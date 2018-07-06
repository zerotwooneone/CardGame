using System;
using System.Collections.Generic;
using CardGame.Core.CQRS;

namespace CardGame.Core.Game
{
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
}