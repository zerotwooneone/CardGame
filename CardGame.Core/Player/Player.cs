using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Player
{
    public class Player: IAggregate<Guid>
    {
        private readonly IEventBus _eventBus;

        public Player(IEventBus eventBus, Guid id)
        {
            _eventBus = eventBus;
            Id = id;
            Apply(new PlayerCreatedEvent {Id = Id});
        }

        private void Apply(IEvent eventObj)
        {
            _eventBus.Broadcast(eventObj);
        }

        public Guid Id { get; }
    }
}
