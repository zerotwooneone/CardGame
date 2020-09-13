using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Utils.Bus
{
    public class RequestRegistry : IRequestRegistryBuilder, IResponseRegistry
    {
        public IReadOnlyDictionary<string, ResponseRegistration> ResponseRegistry { get; private set; } = new Dictionary<string, ResponseRegistration>();

        public void Configure(IReadOnlyDictionary<string, RequestConfiguration> registry, Func<Type, object> resolve)
        {
            var nameToServiceType = new Dictionary<string, Type>();
            var sr = registry.Aggregate(new Dictionary<string, ResponseRegistration>(), (responseRegistrations, currentRequestRegistration) =>
            {
                var serviceName = currentRequestRegistration.Value.Service;
                Type serviceType;
                if (nameToServiceType.TryGetValue(serviceName, out var t))
                {
                    serviceType = t;
                }
                else
                {
                    try
                    {
                        serviceType = GetTypeFromTypeName(serviceName);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Could not find service type {serviceName}. Registry entry {currentRequestRegistration.Key}", e);
                    }

                    try
                    {
                        //resolve the service now, so that the app will not start up if the registry is invalid.
                        var service = resolve(serviceType);
                        //Immediately discard this object
                        service = null;
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Could not resolve service {serviceName}. Registry entry {currentRequestRegistration.Key}", e);
                    }

                    nameToServiceType[serviceName] = serviceType;
                }
                responseRegistrations.Add(currentRequestRegistration.Key, new ResponseRegistration(serviceType, ()=>resolve(serviceType), currentRequestRegistration.Value.Method, currentRequestRegistration.Value.ResponseTopic));
                return responseRegistrations;
            });

            ResponseRegistry = sr;
        }

        private static Type GetTypeFromTypeName(string serviceName)
        {
            var serviceType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(serviceName))
                .FirstOrDefault(t => t != null);
            return serviceType;
        }
    }
}