using System;
using System.Linq;
using CardGame.Peer;
using MassTransit;
using Unity;
using Unity.Lifetime;

namespace CardGamePeer
{
    public class Startup
    {
        public void Start(ProgramViewmodel viewmodel)
        {
            viewmodel.Start();
        }

        public void Setup(IUnityContainer container)
        {
            container.RegisterType<OutputService>(new ContainerControlledLifetimeManager());
            
            // scan for types
            var type = typeof(IConsumer);
            var consumerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            foreach (var consumerType in consumerTypes)
            {
                container.RegisterType(consumerType, new ContainerControlledLifetimeManager());
            }
        }

        public void Configure(OutputService outputService)
        {
            
        }
    }
}