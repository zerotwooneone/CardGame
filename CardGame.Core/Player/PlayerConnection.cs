using System;
using System.Threading.Tasks;
using CardGame.Core.CQRS;

namespace CardGame.Core.Player
{
    public class PlayerConnection : IAggregate<long>
    {
        private readonly EventBroadcaster _eventBroadcaster;
        public long Id { get; }
        public Guid? PlayerId { get; private set; }

        public PlayerConnection(EventBroadcaster eventBroadcaster, long id)
        {
            _eventBroadcaster = eventBroadcaster;
            Id = id;
        }

        public void SetId(Guid id)
        {
            if (PlayerId.HasValue)
            {
                throw new ArgumentException("Id is already set.");
            }

            PlayerId = id;
            Broadcast(new PlayerIdSetEvent(PlayerId.Value));
        }

        private Task<EventResponse> Broadcast(IEvent eventObj)
        {
            return _eventBroadcaster.Broadcast(eventObj);
        }
    }
}