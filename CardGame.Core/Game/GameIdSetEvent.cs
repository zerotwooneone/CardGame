using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Game
{
    public class GameIdSetEvent : IEvent
    {
        public Guid Id { get; }

        public GameIdSetEvent(Guid id)
        {
            Id = id;
        }
    }
}