using System;
using System.Threading.Tasks;
using CardGame.Core.CQRS;

namespace CardGame.Core.Player
{
    public class Player : IAggregate<Guid?>
    {
        private readonly EventBroadcaster _eventBroadcaster;

        public Player(EventBroadcaster eventBroadcaster)
        {
            _eventBroadcaster = eventBroadcaster;
            Broadcast(new PlayerCreatedEvent());
        }

        public Player(EventBroadcaster eventBroadcaster, Guid id)
        {
            _eventBroadcaster = eventBroadcaster;
            Id = id;
        }

        private Task<EventResponse> Broadcast(IEvent eventObj)
        {
            return _eventBroadcaster.Broadcast(eventObj);
        }

        public Guid? Id { get; private set; }

        
    }
}
