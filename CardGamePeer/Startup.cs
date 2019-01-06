using CardGame.Peer;
using Unity;
using Unity.Lifetime;

namespace CardGamePeer
{
    public class Startup
    {
        public void Setup(IUnityContainer container)
        {
            container.RegisterType<OutputService>(new ContainerControlledLifetimeManager());
        }

        public void Configure(OutputService outputService)
        {
            
        }
    }
}