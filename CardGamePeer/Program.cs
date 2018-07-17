using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Core.CQRS;
using CardGame.Core.Game;
using CardGame.Core.Player;
using CardGame.Peer;
using CardGame.Peer.MessagePipe;
using CardGame.Peer.NamedPipes;
using CardGame.Peer.PlayerConnection;
using CardGame.Peer.ResponsePipe;
using Newtonsoft.Json;
using Unity;
using Unity.Lifetime;
using EventHandler = CardGame.Core.CQRS.EventHandler;

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
                const string PipeServername = ".";
                const string MytestPipeName = "MyTest.Pipe";
                var namedPipeConfig = new NamedPipeConfig { PipeName = MytestPipeName, ServerName = PipeServername };
                
                var eventWrapperService = container.Resolve<EventWrapperService>();
                eventWrapperService.AddEventType<PlayerAddedEvent>()
                    .AddEventType<GameCreatedEvent>()
                    .AddEventType<PlayerCreatedEvent>()
                    .AddEventType<GameIdSetEvent>();
                var eventHandler = container.Resolve<EventHandler>();

                EventResponse GenericSuccess(IEvent e) => new EventResponse {Success = true};

                eventHandler
                    .AddHandler<GameCreatedEvent>(e => new EventResponse { Success = true, CreatedId = Guid.NewGuid().ToString()})
                    .AddHandler<PlayerAddedEvent>((Func<IEvent, EventResponse>) GenericSuccess)
                    .AddHandler<PlayerCreatedEvent>((Func<IEvent, EventResponse>) GenericSuccess)
                    .AddHandler<GameIdSetEvent>((Func<IEvent, EventResponse>) GenericSuccess);
                
                var playerConnectionFactory = container.Resolve<PlayerConnectionFactory>();
                var player = playerConnectionFactory.Create(namedPipeConfig).Result;

                var gameFactory = container.Resolve<GameFactory>();
                var game = gameFactory.CreateNew();

                outputService.WriteLine("done");
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
