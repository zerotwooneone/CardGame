using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CardGame.Core.CQRS;
using CardGame.Peer.MessagePipe;
using Newtonsoft.Json;

namespace CardGame.Peer.EventBus
{
    public class RemoteEventBroadcaster
    {
        private readonly ResponsePipe.ResponsePipe _responsePipe;
        private readonly EventWrapperService _eventWrapperService;

        public RemoteEventBroadcaster(ResponsePipe.ResponsePipe responsePipe,
            EventWrapperService eventWrapperService)
        {
            _responsePipe = responsePipe;
            _eventWrapperService = eventWrapperService;
        }

        public Task<EventResponse> Broadcast(IEvent eventObj)
        {
            var eventWrapper = _eventWrapperService.Wrap(eventObj);
            return _responsePipe.GetResponse(new Message
            {
                Id = Guid.NewGuid(),
                Event = eventWrapper
            }).ContinueWith(t => new EventResponse { Success = t.Result.Success, CreatedId = t.Result.CreatedId });
        }
    }
}
