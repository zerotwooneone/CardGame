using System;
using System.Threading.Tasks;
using CardGame.Peer;
using CardGame.Peer.MessagePipe;
using CardGame.Peer.NamedPipes;
using Newtonsoft.Json;
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

            startup.Configure();

            var programViewmodel = container.Resolve<ProgramViewmodel>(); //yuck, fix this later
            programViewmodel.OutputObservable.Subscribe(s => Console.WriteLine(s));
            startup.Start(programViewmodel);

            var outputService = container.Resolve<OutputService>();

            var factory = container.Resolve<MessagePipeFactory>();
            const string PipeServername = ".";
            const string MytestPipeName = "MyTest.Pipe";
            var namedPipeConfig = new NamedPipeConfig { PipeName = MytestPipeName, ServerName = PipeServername };
            var messagePipe = factory.GetMessagePipe(namedPipeConfig).Result;
            messagePipe.MessageObservable.Subscribe(m =>
            {
                outputService.WriteLine($"Message Received:{JsonConvert.SerializeObject(m)}");
            });
            var message = new Message { Id = Guid.NewGuid() };
            outputService.WriteLine($"Sending: {JsonConvert.SerializeObject(message)}");
            messagePipe.SendMessage(message).Wait();

            do
            {
                //Thread.Sleep(TimeSpan.FromMilliseconds(1));
                Task.Delay(TimeSpan.FromMilliseconds(1)).Wait();
            } while (true);
        }


    }
}
