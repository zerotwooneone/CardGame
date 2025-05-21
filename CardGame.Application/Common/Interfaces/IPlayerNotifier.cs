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
    
    // --- Game Group Broadcasts ---
    
    Task BroadcastGameWinnerAsync(Guid gameId, Guid winnerId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Broadcasts the summary of a completed round.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <param name="summaryData">The round summary DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task BroadcastRoundSummaryAsync(Guid gameId, RoundEndSummaryDto summaryData, CancellationToken cancellationToken);

    /// <summary>
    /// Sends information about a card revealed by a Priest effect to the player who played the Priest.
    /// </summary>
    /// <param name="priestPlayerId">The ID of the player who played the Priest.</param>
    /// <param name="targetPlayerId">The ID of the player whose card was revealed.</param>
    /// <param name="targetPlayerName">The name of the player whose card was revealed.</param>
    /// <param name="revealedCard">The card that was revealed from the target player's hand.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendPriestRevealAsync(Guid priestPlayerId, Guid targetPlayerId, string targetPlayerName, CardDto revealedCard, CancellationToken cancellationToken);
}