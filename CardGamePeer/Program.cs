using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            container.RegisterType<Startup>(new ContainerControlledLifetimeManager());
            container.RegisterType<OutputService>(new ContainerControlledLifetimeManager());

            var startup = container.Resolve<Startup>();

            startup.Setup(container);

            startup.Configure();

            var programViewmodel = container.Resolve<ProgramViewmodel>(); //yuck, fix this later
            programViewmodel.OutputObservable.Subscribe(s => Console.WriteLine(s));
            startup.Start(programViewmodel);

            var outputService = container.Resolve<OutputService>();

            try
            {
                using (var c = container.Resolve<HelloWorldClient>())
                {
                    c.Connect(TimeSpan.FromSeconds(1.0));
                    c.SendMessage(new Message { Id = -999 });

                    Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                }
            }
            catch (TimeoutException e) when (e.Message.StartsWith("The operation has timed out."))
            {
                outputService.WriteLine("Client failed to connect. Assuming server role");
                //using (
                var s = container.Resolve<HelloWorldServer>();
                //){
                s.ClientConnectedObservable
                    //.Take(1)
                    //.Delay(TimeSpan.FromMilliseconds(1))
                    .Subscribe(i =>
                {
                    s.SendMessage(new Message { Id = 88 });
                });
                Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                //}
            }

            do
            {
                //Thread.Sleep(TimeSpan.FromMilliseconds(1));
                Task.Delay(TimeSpan.FromMilliseconds(1)).Wait();
            } while (true);
        }


    }
}
