using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            var programViewmodel = container.Resolve<ProgramViewmodel>(); //yuck, fix this later

            var outputService = container.Resolve<OutputService>();
            startup.Configure(outputService);

            programViewmodel.Run().Wait();
            
            Console.WriteLine("\n\n");
            Console.WriteLine("Press enter to quit");
            Console.ReadLine();
        }
    }
}
