using System;
using CardGame.Core.CQRS;

namespace Lobby
{
    public class LobbyIdSetEvent : IEvent
    {
        public Guid Id { get; }

        public LobbyIdSetEvent(Guid id)
        {
            Id = id;
        }
    }
}