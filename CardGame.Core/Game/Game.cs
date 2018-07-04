using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CardGame.Core.CQRS;

namespace CardGame.Core.Game
{
    public class Game : IAggregate<Guid>
    {
        private readonly IEventBus _eventBus;
        private readonly IProducerConsumerCollection<Guid> _players;

        public Game(IEventBus eventBus, Guid id, Guid playerId)
        {
            _eventBus = eventBus;
            Id = id;
            _players = new ConcurrentBag<Guid>(new[]{ playerId });
            Broadcast(new GameCreatedEvent(Id, Players));
        }

        public Guid Id { get; }
        public EventResponse Broadcast(IEvent eventObj)
        {
            return _eventBus.Broadcast(eventObj);
        }

        public IEnumerable<Guid> Players => _players;

        public void AddPlayer(Guid playerId)
        {
            _players.TryAdd(playerId);
            Broadcast(new PlayerAddedEvent(playerId));
        }
    }

    public class GameCreatedEvent : IEvent
    {
        public GameCreatedEvent(Guid gameId, IEnumerable<Guid> players)
        {
            GameId = gameId;
            Players = players;
        }

        public Guid GameId { get; set; }
        public IEnumerable<Guid> Players { get; set; }
    }

    public class PlayerAddedEvent : IEvent
    {
        public PlayerAddedEvent(Guid playerId)
        {
            PlayerId = playerId;
        }

        public Guid PlayerId { get; set; }
    }
}
