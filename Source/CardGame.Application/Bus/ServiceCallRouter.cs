using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Application.Bus
{
    public class ServiceCallRouter : IServiceCallRouter
    {
        private readonly IResponseRegistry _responseRegistry;

        public ServiceCallRouter(IResponseRegistry responseRegistry)
        {
            _responseRegistry = responseRegistry;
        }
        public Task Route(ServiceCall serviceCall)
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
    }
}
