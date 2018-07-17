using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CardGame.Core.CQRS;
using CardGame.Peer.MessagePipe;
using CardGame.Peer.NamedPipes;
using Newtonsoft.Json;

namespace CardGame.Peer.PlayerConnection
{
    public class PlayerConnectionFactory
    {
        private readonly MessagePipeFactory _messagePipeFactory;
        private readonly OutputService _outputService;
        private readonly EventWrapperService _eventWrapperService;
        private readonly Core.CQRS.EventHandler _eventHandler;
        private readonly EventBroadcaster _eventBroadcaster;

        public PlayerConnectionFactory(MessagePipeFactory messagePipeFactory,
            OutputService outputService,
            EventWrapperService eventWrapperService,
            Core.CQRS.EventHandler eventHandler,
            EventBroadcaster eventBroadcaster)
        {
            _messagePipeFactory = messagePipeFactory;
            _outputService = outputService;
            _eventWrapperService = eventWrapperService;
            _eventHandler = eventHandler;
            _eventBroadcaster = eventBroadcaster;
        }

        public async Task<Core.Player.PlayerConnection> Create(NamedPipeConfig namedPipeConfig)
        {
            var messagePipe = await _messagePipeFactory.GetMessagePipe(namedPipeConfig).ConfigureAwait(false);
            var messageHandler = new MessageHandler(messagePipe);

            var messageHandlers = new List<HandlerConfig>();

            bool GetAllMessages(Message m) => true;

            messageHandlers.Add(
                new HandlerConfig
                {
                    Filter = GetAllMessages,
                    Handler = m =>
                    {
                        _outputService.WriteLine($"message received:{JsonConvert.SerializeObject(m)}");
                    }
                }
            );

            bool GetEventMessages(Message m) => m.Response == null && m.Event != null;

            messageHandlers.Add(new HandlerConfig
            {
                Filter = GetEventMessages
            });

            bool EventFilter(Message m) => m.Response == null && m.Event != null;

            Dictionary<Func<Message, bool>, Func<Message, Task<Response>>> responseHandlers =
                new Dictionary<Func<Message, bool>, Func<Message, Task<Response>>>()
                {
                    {
                        EventFilter,
                        async m =>
                        {
                            var eventObj = _eventWrapperService.UnWrap(m.Event);
                            _outputService.WriteLine($"upwrapped type:{eventObj.GetType()} event: {eventObj}");
                            var eventResponse = await _eventHandler.Handle(eventObj);
                            var response2 = new Response { Id = Guid.NewGuid(), CreatedId = eventResponse.CreatedId, Success = eventResponse.Success };
                            return response2;
                        }
                    }
                };
            messageHandler.RegisterHandlers(messageHandlers, responseHandlers);

            var responsePipe = new ResponsePipe.ResponsePipe(messagePipe);
            var responseSubscription = _eventBroadcaster
                .BroadcastObservable
                .Select(e =>
                {
                    var message = new Message { Event = _eventWrapperService.Wrap(e), Id = Guid.NewGuid() };
                    return responsePipe.GetResponse(message);
                })
                .Subscribe(async tr =>
                {
                    var response = await tr.ConfigureAwait(false);
                    //_eventHandler.Handle(response);
                    _outputService.WriteLine($"got event response:{JsonConvert.SerializeObject(response)}");
                });

            return new Core.Player.PlayerConnection(_eventBroadcaster, GetNextConnectionId());
        }

        private static long _nextConnectionId = 0; //this is a hack to save time for now
        private long GetNextConnectionId()
        {
            return _nextConnectionId++;
        }
    }
}
