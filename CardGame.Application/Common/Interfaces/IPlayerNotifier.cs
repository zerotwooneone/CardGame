using CardGame.Application.DTOs;

namespace CardGame.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for sending notifications directly to specific players.
/// </summary>
public interface IPlayerNotifier
{
    /// <summary>
    /// Sends the current hand state to a specific player.
    /// </summary>
    /// <param name="playerId">The ID of the player to notify.</param>
    /// <param name="handCards">The list of cards currently in the player's hand.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendHandUpdateAsync(Guid playerId, List<CardDto> handCards, CancellationToken cancellationToken);

    // Add other methods for player-specific notifications
    // Example: Task SendPriestRevealAsync(Guid requestingPlayerId, Guid opponentId, CardDto revealedCard, CancellationToken cancellationToken);
}