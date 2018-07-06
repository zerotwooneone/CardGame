using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Player
{
    public class PlayerCreatedEvent : IEvent
    {
        public Guid Id { get; set; }
    }
}