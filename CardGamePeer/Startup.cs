using System;
using CardGame.Peer;
using CardGame.Peer.NamedPipes;
using Unity;
using Unity.Injection;
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
            container.RegisterType<OutputService>(new ContainerControlledLifetimeManager());
            container.RegisterType<Func<NamedPipeConfig, ClientPipe>>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(
                    c =>
                    {
                        ClientPipe GetClientPipe(NamedPipeConfig namedPipeConfig) =>
                            new ClientPipe(namedPipeConfig.ServerName, namedPipeConfig.PipeName);

                        return (Func<NamedPipeConfig, ClientPipe>) GetClientPipe;
                    }));
            container.RegisterType<Func<NamedPipeConfig, ServerPipe>>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(
                    c =>
                    {
                        ServerPipe GetClientPipe(NamedPipeConfig namedPipeConfig) =>
                            new ServerPipe(namedPipeConfig.PipeName);

                        return (Func<NamedPipeConfig, ServerPipe>) GetClientPipe;
                    }));
        }

        public void Configure()
        {
            
        }
    }
}