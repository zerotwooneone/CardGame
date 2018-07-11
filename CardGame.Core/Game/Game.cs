using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CardGame.Core.CQRS;

namespace CardGame.Core.Game
{
    public class Game : IAggregate<Guid>
    {
        private readonly EventBroadcaster _eventBroadcaster;
        private readonly IProducerConsumerCollection<Guid> _players;

        public Game(EventBroadcaster eventBroadcaster, Guid id, Guid playerId)
        {
            _eventBroadcaster = eventBroadcaster;
            Id = id;
            _players = new ConcurrentBag<Guid>(new[]{ playerId });
            Broadcast(new GameCreatedEvent(Id, Players));
        }

        public Guid Id { get; }
        public EventResponse Broadcast(IEvent eventObj)
        {
            return _eventBroadcaster.Broadcast(eventObj);
        }

        public IEnumerable<Guid> Players => _players;

        public void AddPlayer(Guid playerId)
        {
            _players.TryAdd(playerId);
            Broadcast(new PlayerAddedEvent(playerId));
        }
    }
}
