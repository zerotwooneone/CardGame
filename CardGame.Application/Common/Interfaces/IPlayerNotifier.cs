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
}