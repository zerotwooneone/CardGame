using System;
using CardGame.Core.CQRS;

namespace Lobby
{
    public class PlayerAddedEvent : IEvent
    {
        public PlayerAddedEvent(Guid playerId, Guid lobbyId)
        {
            PlayerId = playerId;
            LobbyId = lobbyId;
        }

        public Guid PlayerId { get; }
        public Guid LobbyId { get; }
    }
}