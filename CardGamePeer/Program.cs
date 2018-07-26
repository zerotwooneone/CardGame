﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                        //endpointConfigurator.AutoDelete = true;
                        //endpointConfigurator.PurgeOnStartup = true;
                        
                        endpointConfigurator.Handler<LobbyIdSetEvent>(context =>
                        {
                            outputService.WriteLine($"state machine got : {context.Message.Id}");
                            return Task.CompletedTask;
                        });
                        
                    });
                });

                BusHandle busHandle = TaskUtil.Await(() => bus.StartAsync());
                try
                {
                    var newGuid = Guid.NewGuid();
                    outputService.WriteLine($"creating game id : {newGuid}");
                    var gameIdSetEvent = new LobbyIdSetEvent(newGuid);

                    bus.Publish(gameIdSetEvent);
                    
                    
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

    public static class Extensions
    {
        public static T? FirstOrNull<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) where T : struct
        {
            var first = enumerable.FirstOrDefault(predicate);
            return EqualityComparer<T>.Default.Equals(first, default(T)) ? (T?)null : first;
        }

        public static T? FirstOrNull<T>(this IEnumerable<T> enumerable) where T : struct
        {
            return enumerable.FirstOrNull(t=>true);
        }
    }
}
