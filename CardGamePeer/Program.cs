using System;
using System.Collections.Generic;
using System.Threading;
using CardGame.Core.CQRS;
using CardGame.Core.Game;
using CardGame.Core.Player;
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
            var messageHandler = new MessageHandler(messagePipe);
            var handlerConfigs = new List<HandlerConfig>();
            handlerConfigs.Add(
                new HandlerConfig
                {
                    Filter = m => true,
                    Handler = m => { outputService.WriteLine($"X message received:{JsonConvert.SerializeObject(m)}"); }
                }
            );
            var eventWrapperService = container.Resolve<EventWrapperService>();
            eventWrapperService.AddEventType<PlayerAddedEvent>();
            eventWrapperService.AddEventType<GameCreatedEvent>();
            eventWrapperService.AddEventType<PlayerCreatedEvent>();
            var eventBus = container.Resolve<CardGame.Core.CQRS.EventHandler>();

            eventBus.AddHandler<GameCreatedEvent>(e => new EventResponse { Success = true });
            eventBus.AddHandler<PlayerAddedEvent>(e => new EventResponse { Success = true });
            eventBus.AddHandler<PlayerCreatedEvent>(e => new EventResponse { Success = true });

            handlerConfigs.Add(new HandlerConfig
            {
                Filter = m => m.Response == null && m.Event != null
            });
            Dictionary<Func<Message, bool>, Func<Message, Response>> readOnlyDictionary =
                new Dictionary<Func<Message, bool>, Func<Message, Response>>()
            {
                {
                    m=>m.Response == null && m.Event !=null,
                    m =>
                    {
                        var eventObj = eventWrapperService.UnWrap(m.Event);
                        outputService.WriteLine($"upwrapped type:{eventObj.GetType()} event: {eventObj}");
                        var eventResponse = eventBus.Handle(eventObj);
                        var response2 = new Response { Id = Guid.NewGuid(), CreatedId = eventResponse.CreatedId, Success = eventResponse.Success };
                        return response2;
                    }
                }
            };
            messageHandler.RegisterHandlers(handlerConfigs, readOnlyDictionary);
            var responsePipe = new ResponsePipe(messagePipe);

            var eventMessage = new Message { Id = Guid.NewGuid(), Event = eventWrapperService.Wrap(new GameCreatedEvent(Guid.NewGuid(), new List<Guid>())) };
            var response4 = responsePipe.GetResponse(eventMessage).Result;
            outputService.WriteLine($"Response:{JsonConvert.SerializeObject(response4)}");

            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            } while (true);
        }


    }
}
