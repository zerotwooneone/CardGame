using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Utils.Bus
{
    public class NaiveBus: IBus
    {
        private readonly ISubject<ICommonEvent> _eventSubject;
        private readonly IEventConverter _eventConverter;

        public NaiveBus(ISubject<ICommonEvent> eventSubject,
            IServiceCallRouter serviceCallRouter, 
            IEventConverter eventConverter)
        {
            _eventSubject = eventSubject;
            _eventConverter = eventConverter;

            //todo: register service calls somewhere else
            var serviceCallSubscription = Subscribe<ServiceCall>("ServiceCall", sc =>
            {
                serviceCallRouter.Route(sc);
            });
        }
        public void Publish<T>(string topic, 
            T obj, 
            Guid correlationId = default, 
            Guid eventId = default)
        {
            if (!_eventConverter.CanConvert(topic))
            {
                throw new Exception($"topic not registered:{topic}");
            }
            if (eventId == default(Guid))
            {
                eventId = Guid.NewGuid();
            }

            var cid = correlationId == default
                ? (Guid?)null
                : correlationId;
            var values = _eventConverter.GetValues(topic, obj);
            _eventSubject.OnNext(new CommonEvent
            {
                EventId = eventId,
                CorrelationId = cid,
                Values = values,
                Topic = topic
            });
        }
        private static readonly IReadOnlyDictionary<string, RequestRegistration> RequestRegistry = new Dictionary<string, RequestRegistration>
        {
            {"CardGame.Domain.Abstractions.Game.IGameService:NextRound", new RequestRegistration
                {
                    Service = "CardGame.Domain.Abstractions.Game.IGameService", 
                    Method = "NextRound", 
                    ResponseTopic = "RoundStarted"
                }
            },
            {"CardGame.Domain.Abstractions.Game.IPlayService:Play", new RequestRegistration
                {
                    Service = "CardGame.Domain.Abstractions.Game.IPlayService", 
                    Method = "Play", 
                    ResponseTopic = "CardPlayed"
                }
            }
        };
        public Task<TResponse> Request<TRequest, TResponse>(string requestTopic,
            Guid correlationId,
            TRequest value,
            CancellationToken cancellationToken = default)
        {
            if (!RequestRegistry.TryGetValue(requestTopic, out var registration))
            {
                throw new ArgumentException($"request topic not found: {requestTopic}", nameof(requestTopic));
            }
            var serviceResponseTimeout = TimeSpan.FromMinutes(1); //todo: make this configurable
            var timeoutSource = new CancellationTokenSource(serviceResponseTimeout);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
            var token = cts.Token;

            var tcs = new TaskCompletionSource<TResponse>();
            token.Register(() =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.SetCanceled();
                }
            });
            
            var converted = CreateConvertedObservable<TResponse>(registration.ResponseTopic);

            void OnNext(TResponse response)
            {
                if (timeoutSource.IsCancellationRequested)
                {
                    tcs.SetException(new Exception("service response timeout occurred"));
                }
                else
                {
                    tcs.SetResult(response);
                }
            }

            void OnCompleted()
            {
                if (!tcs.Task.IsCompleted)
                {
                    if (timeoutSource.IsCancellationRequested)
                    {
                        tcs.SetException(new Exception("service response timeout occurred"));
                    }
                    else
                    {
                        tcs.SetCanceled();
                    }
                }
            }

            converted
                .Take(1)
                .Subscribe(OnNext, OnCompleted, token);

            Publish("ServiceCall", new ServiceCall
            {
                Service = registration.Service,
                Method = registration.Method,
                Param = value,
            }, correlationId);

            return tcs.Task;
        }

        public ISubscription Subscribe<T>(string topic, 
            Action<T> handler)
        {
            var converted = CreateConvertedObservable<T>(topic);
            var subscription = converted 
                .Subscribe(handler);
            var result = new SubscriptionWrapper(subscription);
            return result;
        }

        private IObservable<T> CreateConvertedObservable<T>(string topic)
        {
            if (!_eventConverter.CanConvert(topic))
            {
                throw new Exception($"topic not registered:{topic}");
            }
            T Convert(ICommonEvent commonEvent)
            {
                var obj = _eventConverter.GetObject<T>(topic, commonEvent.Values, commonEvent.EventId,
                    commonEvent.CorrelationId);
                return obj;
            }

            var converted = _eventSubject
                .Where(ce => ce.Topic == topic)
                .Select(Convert);
            return converted;
        }

        internal class RequestRegistration
        {
            public string Service { get; set; }
            public string Method { get; set; } 
            public string ResponseTopic { get; set; }
        }
    }
}
