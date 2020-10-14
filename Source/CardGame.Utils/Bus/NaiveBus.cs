using System;
using System.Reactive;
using System.Reactive.Concurrency;
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
        private readonly IEventConverter _eventConverter;
        private readonly IResponseRegistry _responseRegistry;

        public NaiveBus(IEventConverter eventConverter,
            IResponseRegistry responseRegistry)
        {
            _eventConverter = eventConverter;
            _responseRegistry = responseRegistry;
        }

        public async Task Publish(string topic, 
            object obj, 
            Guid correlationId = default, 
            Guid eventId = default)
        {
            if (!_eventConverter.CanConvert(topic))
            {
                throw new Exception($"topic not registered:{topic}");
            }
            if (eventId == default)
            {
                eventId = Guid.NewGuid();
            }

            var cid = correlationId == default
                ? (Guid?)null
                : correlationId;
            var values = _eventConverter.GetValues(topic, obj);
            await _eventConverter.Publish(topic, new CommonEvent
            {
                EventId = eventId,
                CorrelationId = cid,
                Values = values,
                Topic = topic
            });
        }
        public async Task<TResponse> Request<TResponse>(string requestTopic,
            object value,
            Guid correlationId, 
            Guid eventId = default,
            CancellationToken cancellationToken = default)
        {
            if (!_responseRegistry.ResponseRegistry.TryGetValue(requestTopic, out var responseRegistration))
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
                tcs.TrySetCanceled();
            });

            ISubscription responseSubscription = null;
            token.Register(() => responseSubscription?.Dispose());

            Task HandleResponse(TResponse response)
            {
                tcs.TrySetResult(response);
                return Task.CompletedTask;
            }

            responseSubscription = this.SubscribeToFirst<TResponse>(responseRegistration.ResponseTopic, HandleResponse);

            var publishId = eventId == Guid.Empty
                ? Guid.NewGuid()
                : eventId;

            ISubscription rejectionSubscription = null;
            token.Register(() => rejectionSubscription?.Dispose());

            Task HandleRejection(Rejected rejected)
            {
                if (rejected.OriginalEventId == publishId)
                {
                    tcs.TrySetCanceled();
                }
                return Task.CompletedTask;
            }
            rejectionSubscription = this.SubscribeToFirst<Rejected>(nameof(Rejected), HandleRejection);

            ISubscription failSubscription = null;
            token.Register(() => failSubscription?.Dispose());

            Task HandleServiceFailure(ServiceCallFailed scf)
            {
                if (scf.ServiceCallEventId == publishId && scf.CorrelationId == correlationId)
                {
                    tcs.TrySetException(new Exception("ServiceCallFailed", new Exception(scf.Exception)));
                }

                return Task.CompletedTask;
            }

            failSubscription = this.SubscribeToFirst<ServiceCallFailed>("ServiceCallFailed", HandleServiceFailure);

            await Publish("ServiceCall", new ServiceCall
                {
                    RequestTopic = requestTopic,
                    Param = value,
                    CorrelationId = correlationId,
                    EventId = publishId,
                }, 
                correlationId,
                eventId: publishId);

            return await tcs.Task;
        }

        public ISubscription Subscribe<T>(string topic, 
            Func<T, Task> handler)
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

            return _eventConverter.Subscribe(topic, async commonEvent =>
            {
                var converted = Convert(commonEvent);
                await handler(converted);
            });
        }
    }
}
