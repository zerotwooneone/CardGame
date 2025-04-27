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
    Task SendPriestRevealAsync(Guid requestingPlayerId, Guid opponentId, CardDto revealedCard, CancellationToken cancellationToken);
    
    // --- Game Group Broadcasts ---
    Task BroadcastGuardGuessAsync(Guid gameId, Guid guesserId, Guid targetId, int guessedCardType, bool wasCorrect, CancellationToken cancellationToken);
    Task BroadcastBaronComparisonAsync(Guid gameId, Guid player1Id, int player1CardType, Guid player2Id, int player2CardType, Guid? loserId, CancellationToken cancellationToken);
    Task BroadcastPlayerDiscardAsync(Guid gameId, Guid targetPlayerId, CardDto discardedCard, CancellationToken cancellationToken);
    Task BroadcastKingSwapAsync(Guid gameId, Guid player1Id, Guid player2Id, CancellationToken cancellationToken);

    Task BroadcastRoundWinnerAsync(Guid gameId, Guid? winnerId, string reason, Dictionary<Guid, int?> finalHands,
        CancellationToken cancellationToken);

    Task BroadcastGameWinnerAsync(Guid gameId, Guid winnerId, CancellationToken cancellationToken);
}