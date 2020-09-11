using System;
using System.Linq;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Application.Bus
{
    public class ServiceCallRouter : IServiceCallRouter
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceCallRouter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task Route(ServiceCall serviceCall)
        {
            var serviceType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(serviceCall.Service))
                .FirstOrDefault(t => t != null);
            var service = _serviceProvider.GetService(serviceType);
            
            var method = serviceType.GetMethod(serviceCall.Method);
            var result = method.Invoke(service, new object[]{serviceCall.Param});
            
            var task = result as Task;
            if (task != null)
            {
                await task;
            }
        }
    }
}
