using System;
using System.Reactive;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Application.Bus
{
    public class ServiceCallRouter : IServiceCallRouter
    {
        private readonly IResponseRegistry _responseRegistry;
        private readonly IBus _bus;

        public ServiceCallRouter(IResponseRegistry responseRegistry,
            IBus bus)
        {
            _responseRegistry = responseRegistry;
            _bus = bus;
        }
        public Task Invoke(ServiceCall serviceCall)
        {
            var responseRegistration = _responseRegistry.ResponseRegistry[serviceCall.RequestTopic];
            var serviceType = responseRegistration.ServiceType;
            var service = responseRegistration.Resolve();

            var method = serviceType.GetMethod(responseRegistration.Method);
            var result = method.Invoke(service, new object[]{serviceCall.Param});
            
            var task = result as Task;
            if (task != null)
            {
                return task;
            }
            else
            {
                return Task.FromResult(result);
            }
        }

        public void Configure()
        {
            var serviceCallSubscription = _bus.Subscribe<ServiceCall>("ServiceCall", OnServiceCall);
        }

        private async Task<Unit> OnServiceCall(ServiceCall sc)
        {
            //todo: need to handle errors in the observable
            var task = Invoke(sc);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                _bus.PublishEvent("ServiceCallFailed", ServiceCallFailed.Factory(sc, task.Exception));
                return Unit.Default;
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
                        //do not do this: Publish(responseRegistration.ResponseTopic, result, sc.CorrelationId);
                    }
                    //todo: handle more service return types
                }
            }
            catch (Exception e)
            {
                _bus.PublishEvent("ServiceCallFailed", ServiceCallFailed.Factory(sc, e, responseRegistration?.ServiceType.ToString(), responseRegistration?.Method));
            }
            return Unit.Default;
        }
    }
}
