using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Player
{
    public class Player: IAggregate<Guid>
    {
        private readonly EventBroadcaster _eventBroadcaster;

        public Player(EventBroadcaster eventBroadcaster, Guid id)
        {
            _eventBroadcaster = eventBroadcaster;
            Id = id;
            Apply(new PlayerCreatedEvent {Id = Id});
        }

        private void Apply(IEvent eventObj)
        {
            _eventBroadcaster.Broadcast(eventObj);
        }

        public Guid Id { get; }
    }
}
