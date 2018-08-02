using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Core.Challenge;
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
                var lowHighChallengeFactory = container.Resolve<LowHighChallengeFactory>();
                Guid source = Guid.NewGuid();
                Guid target = Guid.NewGuid();
                var cryptoService = container.Resolve<ICryptoService>();
                var lhSm = new LowHighChallengeStateMachine(lowHighChallengeFactory, source, cryptoService);
                var targetSm = new LowHighChallengeStateMachine(lowHighChallengeFactory, target, cryptoService);
                var inMemorySagaRepository = new InMemorySagaRepository<LowHighChallenge>();
                var bus = Bus.Factory
                    .CreateUsingRabbitMq(factoryConfigurator =>
                    {
                        var host = factoryConfigurator.Host(new Uri("rabbitmq://localhost"), hostConfigurator =>
                        {
                            hostConfigurator.Username("guest");
                            hostConfigurator.Password("guest");
                        });

                        factoryConfigurator.ReceiveEndpoint(host, "game_state", endpointConfigurator =>
                        {
                            endpointConfigurator.PrefetchCount = 8;
                            endpointConfigurator.UseInMemoryOutbox();
                            endpointConfigurator.AutoDelete = true;
                            endpointConfigurator.PurgeOnStartup = true;

                            endpointConfigurator.Handler<LobbyIdSetEvent>(context =>
                            {
                                outputService.WriteLine($"state machine got : {context.Message.Id}");
                                return Task.CompletedTask;
                            });

                            endpointConfigurator.StateMachineSaga(lhSm, inMemorySagaRepository);
                            endpointConfigurator.StateMachineSaga(targetSm, new InMemorySagaRepository<LowHighChallenge>());
                        });
                    });

                BusHandle busHandle = TaskUtil.Await(() => bus.StartAsync());
                try
                {
                    //var newGuid = Guid.NewGuid();
                    //outputService.WriteLine($"creating game id : {newGuid}");
                    //var gameIdSetEvent = new LobbyIdSetEvent(newGuid);

                    //bus.Publish(gameIdSetEvent);

                    
                    outputService.WriteLine($"source: {source}{Environment.NewLine} target:{target}");
                    var challenge = lowHighChallengeFactory.CreateRequest(source, target, bus);
                    //inMemorySagaRepository.Add(new SagaInstance<LowHighChallenge>(challenge), CancellationToken.None ).Wait();
                    //var currentState = challenge.CurrentState;
                    //outputService.WriteLine($"{nameof(currentState)}:{currentState}");
                    var count = (int)TimeSpan.TicksPerSecond * 2;
                    var delay = TimeSpan.FromTicks(1);
                    //foreach (var i in Enumerable
                    //    .Range(1, count))
                    //{
                        //if (challenge.CurrentState != currentState)
                        //{
                        //    outputService.WriteLine($"{nameof(currentState)}:{currentState}");
                        //    currentState = challenge.CurrentState;
                        //}
                    //    Task.Delay(delay).Wait();
                    //}
                    //var result = challenge.Success.Result;
                    //outputService.WriteLine($"success:{result}");

                    Task.Delay(TimeSpan.FromSeconds(5)).Wait(); //give the bus time to drain
                    outputService.WriteLine("done");
                }
                finally
                {
                    busHandle.Stop();
                }
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
