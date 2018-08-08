using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Core.Challenge;
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
            var winSubject = new Subject<bool>();
            try
            {
                var lowHighChallengeFactory = container.Resolve<LowHighChallengeFactory>();
                var cryptoService = container.Resolve<ICryptoService>();
                var stateMachines = new List<LowHighChallengeStateMachine>();
                var sagaRespositories = new Dictionary<Guid, InMemorySagaRepository<LowHighChallenge>>();
                var isOne = DateTime.Now.Minute == 7;
                //if (isOne)
                //{
                //    Task.Delay(TimeSpan.FromMinutes(1)).Wait();
                //}
                //File.WriteAllLines("one.txt", Enumerable.Range(1,30).Select(i=>Guid.NewGuid().ToString()));
                //File.WriteAllLines("two.txt", Enumerable.Range(1,30).Select(i=>Guid.NewGuid().ToString()));
                var guids = File
                    .ReadAllLines(isOne ? "one.txt" : "two.txt")
                    .Select(Guid.Parse)
                    .Take(30);
                foreach (var source in guids)
                {
                    stateMachines.Add(new LowHighChallengeStateMachine(lowHighChallengeFactory, source, cryptoService));
                    sagaRespositories.Add(source, new InMemorySagaRepository<LowHighChallenge>());
                }
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

                            stateMachines.ForEach(sm =>
                            {
                                endpointConfigurator.StateMachineSaga(sm, sagaRespositories[sm.Id]);
                            });

                            endpointConfigurator
                                .Handler<LowHighChallengeCompletedEvent>(context =>
                                {
                                    winSubject.OnNext(context.Message.RequesterWins);
                                    return Task.CompletedTask;
                                });
                        });
                    });

                BusHandle busHandle = TaskUtil.Await(() => bus.StartAsync());
                try
                {
                    var intObservable = winSubject
                        .Select(b => b ? 1 : 0);
                    var allIntObservable = intObservable
                        .TakeUntil(intObservable.Throttle(TimeSpan.FromSeconds(1)));
                    var avgTask = allIntObservable
                        .Average()
                        .ToTask();

                    var random = new Random();
                    int requestTotal = 0;
                    for (int i = 0; i < stateMachines.Count; i++)
                    {
                        LowHighChallengeStateMachine stateMachine = stateMachines[i];
                        var requestCount = i < (stateMachines.Count / 2) ? 10 : 0;
                        requestTotal += requestCount;
                        for (int requestIndex = 0; requestIndex < requestCount; requestIndex++)
                        {
                            var target = stateMachines[random.Next(0, stateMachines.Count)].Id;
                            while (target == stateMachine.Id)
                            {
                                target = stateMachines[random.Next(0, stateMachines.Count)].Id;
                            }

                            var challenge =
                                lowHighChallengeFactory.CreateRequest(stateMachine.Id, target, bus, c =>
                                {
                                    var repository = sagaRespositories[stateMachine.Id];
                                    repository.Add(new SagaInstance<LowHighChallenge>(c), CancellationToken.None)
                                        .Wait();
                                });

                        }
                    }

                    outputService.WriteLine($"avg:{avgTask.Result}{Environment.NewLine}total requests:{requestTotal}");


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
