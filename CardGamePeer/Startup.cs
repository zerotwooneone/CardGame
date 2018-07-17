using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CardGame.Core.CQRS;
using CardGame.Peer;
using CardGame.Peer.MessagePipe;
using CardGame.Peer.NamedPipes;
using Newtonsoft.Json;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using EventHandler = CardGame.Core.CQRS.EventHandler;

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
            const string staticEventHandler = "Static";
            container.RegisterInstance(staticEventHandler, new MyDictionary());
            container.RegisterType<CardGame.Core.CQRS.EventHandler>(
                new InjectionConstructor(
                    new ResolvedParameter<MyDictionary>(staticEventHandler)));

            const string staticEventBroadcast = "staticEventBroadcast";
            container.RegisterInstance<ISubject<IEvent>>(staticEventBroadcast, new Subject<IEvent>());
            container.RegisterType<EventBroadcaster>(new InjectionConstructor(typeof(CardGame.Core.CQRS.EventHandler),new ResolvedParameter<ISubject<IEvent>>(staticEventBroadcast)));
        }

        public void Configure(OutputService outputService)
        {
            
        }
    }

    public class MyDictionary : Dictionary<Type, Func<IEvent, Task<EventResponse>>>
    {

    }
}