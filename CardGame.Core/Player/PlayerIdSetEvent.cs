using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Player
{
    public class PlayerIdSetEvent : IEvent
    {
        public Guid Id { get; }

        public PlayerIdSetEvent(Guid id)
        {
            Id = id;
        }
    }
}