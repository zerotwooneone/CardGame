using System;
using System.Collections.Generic;
using System.Threading;
using CardGame.Peer;
using CardGame.Peer.MessagePipe;
using CardGame.Peer.NamedPipes;
using CardGame.Peer.ResponsePipe;
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
            var responsePipe = new ResponsePipe(messagePipe);
            var messageHandler = new MessageHandler(messagePipe);
            var magicGuid = Guid.Parse("00000000000000000000000000000001");
            var handlerConfigs = new HandlerConfig[] {
            //    new HandlerConfig { Filter = m => m.Id == magicGuid, Handler = m =>
            //    {
            //        outputService.WriteLine($"message handler:{JsonConvert.SerializeObject(m)}");
            //    }
            //}
            };
            Dictionary<Func<Message, bool>, Func<Message, Response>> readOnlyDictionary = new Dictionary<Func<Message, bool>, Func<Message, Response>> { { m => m.Id == magicGuid, m => new Response { Id = Guid.NewGuid() } } };
            messageHandler.RegisterHandlers(handlerConfigs, readOnlyDictionary);
            var response = responsePipe.GetResponse(new Message { Id = magicGuid }).Result;
            outputService.WriteLine($"Response:{JsonConvert.SerializeObject(response)}");

            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            } while (true);
        }


    }
}
