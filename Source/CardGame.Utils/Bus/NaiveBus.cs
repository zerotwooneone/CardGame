using System;
using System.Reactive;
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
        private readonly IServiceCallRouter _serviceCallRouter;
        private readonly IEventConverter _eventConverter;
        private readonly IResponseRegistry _responseRegistry;

        public NaiveBus(ISubject<ICommonEvent> eventSubject,
            IServiceCallRouter serviceCallRouter, 
            IEventConverter eventConverter,
            IResponseRegistry responseRegistry)
        {
            _eventSubject = eventSubject;
            _serviceCallRouter = serviceCallRouter;
            _eventConverter = eventConverter;
            _responseRegistry = responseRegistry;

            //todo: register service calls somewhere else
            var converted = CreateConvertedObservable<ServiceCall>("ServiceCall");
            var handled = converted.SelectMany(OnServiceCall);
            handled.Subscribe();
        }

        private async Task<Unit> OnServiceCall(ServiceCall sc)
        {
            //todo: need to handle errors in the observable
            var task = _serviceCallRouter.Route(sc);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                Publish("ServiceCallFailed", ServiceCallFailed.Factory(sc, task.Exception), sc.CorrelationId);
            }
            ResponseRegistration responseRegistration = null;
            try
            {
                if (_responseRegistry.ResponseRegistry.TryGetValue(sc.RequestTopic, out responseRegistration))
                {
                    var taskType = task.GetType();
                    if (taskType.IsGenericType)
                    {
                        var result = taskType.GetProperty("Result")?.GetValue(task);
                        //danger: result may be Task<VoidTaskResult>, which should still be treated as not having a result
                        //todo: figure out how we want to handle service results. Should services be REQUIRED to publish a response? or should they just return their response?
                        //Publish(responseRegistration.ResponseTopic, result, sc.CorrelationId);
                    }
                    //todo: handle more service return types
                }
            }
            catch (Exception e)
            {
                Publish("ServiceCallFailed", ServiceCallFailed.Factory(sc, e, responseRegistration?.ServiceType.ToString(), responseRegistration?.Method), sc.CorrelationId);
            }
            return Unit.Default;
        }

        public void Publish(string topic, 
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
            _eventSubject.OnNext(new CommonEvent
            {
                EventId = eventId,
                CorrelationId = cid,
                Values = values,
                Topic = topic
            });
        }
        public Task<TResponse> Request<TRequest, TResponse>(string requestTopic,
            Guid correlationId,
            TRequest value,
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
                if (!tcs.Task.IsCompleted)
                {
                    tcs.SetCanceled();
                }
            });

            var converted = CreateConvertedObservable<TResponse>(responseRegistration.ResponseTopic);

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

            var publishId = Guid.NewGuid();

            var failObservable = CreateConvertedObservable<ServiceCallFailed>("ServiceCallFailed")
                .Where(sc => sc.ServiceCallEventId == publishId && sc.CorrelationId == correlationId)
                .Take(1);
            failObservable.Subscribe(sc => tcs.TrySetException(new Exception("ServiceCallFailed", new Exception(sc.Exception))));

            Publish("ServiceCall", new ServiceCall
                {
                    RequestTopic = requestTopic,
                    Param = value,
                    CorrelationId = correlationId,
                    EventId = publishId,
                }, 
                correlationId: correlationId,
                eventId: publishId);

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
    }
}
