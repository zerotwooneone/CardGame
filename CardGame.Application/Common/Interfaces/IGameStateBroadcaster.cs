using CardGame.Application.DTOs;

namespace CardGame.Application.Common.Interfaces;

public interface IGameStateBroadcaster
{
    Task BroadcastSpectatorStateAsync(Guid gameId, SpectatorGameStateDto gameState, CancellationToken cancellationToken);
    // Add other broadcast methods if needed (e.g., for player-specific updates)
}