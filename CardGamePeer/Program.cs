using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Automatonymous;
using CardGame.Core.CQRS;
using CardGame.Core.Lobby;
using Lobby;
using CardGame.Core.Player;
using CardGame.Peer;
using CardGame.Peer.MessagePipe;
using CardGame.Peer.NamedPipes;
using CardGame.Peer.PlayerConnection;
using CardGame.Peer.ResponsePipe;
using MassTransit;
using MassTransit.Saga;
using MassTransit.Util;
using Newtonsoft.Json;
using Unity;
using Unity.Lifetime;
using EventHandler = CardGame.Core.CQRS.EventHandler;

namespace CardGamePeer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var container = new UnityContainer();

            container.RegisterType<Startup>(new ContainerControlledLifetimeManager());

            var startup = container.Resolve<Startup>();

            startup.Setup(container);

            var outputService = container.Resolve<OutputService>();
            startup.Configure(outputService);

            var programViewmodel = container.Resolve<ProgramViewmodel>(); //yuck, fix this later
            programViewmodel.OutputObservable.Subscribe(s => Console.WriteLine(s));
            startup.Start(programViewmodel);


            try
            {
                const string PipeServername = ".";
                const string MytestPipeName = "MyTest.Pipe";
                var namedPipeConfig = new NamedPipeConfig { PipeName = MytestPipeName, ServerName = PipeServername };

                var eventWrapperService = container.Resolve<EventWrapperService>();
                eventWrapperService.AddEventType<PlayerAddedEvent>()
                    .AddEventType<LobbyCreatedEvent>()
                    .AddEventType<PlayerCreatedEvent>()
                    .AddEventType<LobbyIdSetEvent>();
                var eventHandler = container.Resolve<EventHandler>();

                EventResponse GenericSuccess(IEvent e) => new EventResponse { Success = true };

                eventHandler
                    .AddHandler<LobbyCreatedEvent>(e => new EventResponse { Success = true, CreatedId = Guid.NewGuid().ToString() })
                    .AddHandler<PlayerAddedEvent>((Func<IEvent, EventResponse>)GenericSuccess)
                    .AddHandler<PlayerCreatedEvent>((Func<IEvent, EventResponse>)GenericSuccess)
                    .AddHandler<LobbyIdSetEvent>((Func<IEvent, EventResponse>)GenericSuccess);

                var gsm = new LobbyStateMachine();
                
                var inMemorySagaRepository = new InMemorySagaRepository<CardGame.Core.Lobby.Lobby>();
                var bus = Bus.Factory
                    .CreateUsingRabbitMq(sbc =>
                {
                    var host = sbc.Host(new Uri("rabbitmq://localhost"), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    sbc.ReceiveEndpoint(host, "game_state", e =>
                    {
                        e.PrefetchCount = 8;
                        e.StateMachineSaga(gsm, inMemorySagaRepository);
                        e.Handler<LobbyIdSetEvent>(context =>
                        {
                            outputService.WriteLine($"state machine got : {context.Message.Id}");
                            return Task.CompletedTask;
                        });
                    });
                });

                var busHandle = TaskUtil.Await(() => bus.StartAsync());
                
                var newGuid = Guid.NewGuid();
                outputService.WriteLine($"creating game id : {newGuid}");
                var gameIdSetEvent = new LobbyIdSetEvent(newGuid);
                
                bus.Publish(gameIdSetEvent);

                outputService.WriteLine("done");
            }
            catch (Exception e)
            {
                outputService.WriteLine(e.ToString());
                Console.ReadLine();
            }

            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            } while (true);
        }


    }
}
