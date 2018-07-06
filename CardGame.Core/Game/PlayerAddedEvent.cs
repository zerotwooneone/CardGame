using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Game
{
    public class PlayerAddedEvent : IEvent
    {
        public PlayerAddedEvent(Guid playerId)
        {
            PlayerId = playerId;
        }

        public Guid PlayerId { get; set; }
    }
}