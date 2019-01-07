using System;
using Autofac;

namespace CardGamePeer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            
            var startup = new Startup();
            startup.Setup(builder);

            var container = builder.Build();
            var programViewmodel = container.Resolve<ProgramViewmodel>(); //yuck, fix this later
            
            programViewmodel.Run().Wait();
            
            Console.WriteLine("\n\n");
            Console.WriteLine("Press enter to quit");
            Console.ReadLine();
        }
    }
}
