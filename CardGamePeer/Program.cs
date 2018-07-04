using System;
using System.Reactive.Linq;
using System.Threading;
using CardGame.Peer.Server;
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

            try
            {
                var c = container.Resolve<HelloWorldClient>();
                c.Connect(TimeSpan.FromSeconds(1.0));
                c.SendMessage(new Message {Id = -999});
            }
            catch (TimeoutException e) when(e.Message.StartsWith("The operation has timed out."))
            {
                var s =container.Resolve<HelloWorldServer>();
                s.ClientConnectedObservable.Take(1).Delay(TimeSpan.FromMilliseconds(1)).Subscribe(i =>
                {
                    s.SendMessage(new Message {Id = 88});
                });
            }

            container.RegisterType<Startup>(new ContainerControlledLifetimeManager());

            var startup = container.Resolve<Startup>();

            startup.Setup(container);

            var outputService = container.Resolve<OutputService>(); //yuck, fix this later
            startup.Configure(outputService);

            var programViewmodel = container.Resolve<ProgramViewmodel>(); //yuck, fix this later
            startup.Start(programViewmodel);

            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            } while (true);
        }

        
    }
}
