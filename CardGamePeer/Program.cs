using System;
using System.Threading;
using Unity;
using Unity.Injection;
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
