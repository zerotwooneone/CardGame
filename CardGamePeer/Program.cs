using System;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Core.Lobby;
using Lobby;
using CardGame.Peer;
using MassTransit;
using MassTransit.Saga;
using MassTransit.Util;
using Unity;
using Unity.Lifetime;


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
                var gsm = new LobbyCreationStateMachine();
                
                var inMemorySagaRepository = new InMemorySagaRepository<CardGame.Core.Lobby.LobbyCreation>();
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
