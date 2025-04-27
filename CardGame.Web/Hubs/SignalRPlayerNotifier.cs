using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Web.Hubs;

/// <summary>
/// Implements IPlayerNotifier using SignalR to send messages via GameHub.
/// </summary>
public class SignalRPlayerNotifier : IPlayerNotifier
{
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private readonly ILogger<SignalRPlayerNotifier> _logger;

    public SignalRPlayerNotifier(IHubContext<GameHub, IGameClient> hubContext, ILogger<SignalRPlayerNotifier> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends the player's hand via SignalR to the specified user ID.
    /// </summary>
    public async Task SendHandUpdateAsync(Guid playerId, List<CardDto> handCards, CancellationToken cancellationToken)
    {
        if (playerId == Guid.Empty || handCards == null) return;

        string userId = playerId.ToString(); // SignalR targets users by string ID
        try
        {
            // Target the specific user based on their PlayerId (which is the SignalR UserIdentifier)
            // Call the UpdatePlayerHand method defined in IGameClient
            await _hubContext.Clients
                .User(userId)
                .UpdatePlayerHand(handCards);

            _logger.LogInformation("Sent hand update to Player {PlayerId} ({CardCount} cards).", playerId, handCards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending hand update to Player {PlayerId}.", playerId);
            // Decide whether to re-throw or just log
        }
    }
    
    
    /// <summary>
    /// Sends the details of an opponent's revealed card (via Priest) via SignalR to the specified user ID.
    /// </summary>
    public async Task SendPriestRevealAsync(Guid requestingPlayerId, Guid opponentId, CardDto revealedCard, CancellationToken cancellationToken) // Added method implementation
    {
        if (requestingPlayerId == Guid.Empty || opponentId == Guid.Empty || revealedCard == null) return;

        string userId = requestingPlayerId.ToString(); // Target the player who played the Priest
        try
        {
            await _hubContext.Clients
                .User(userId)
                .RevealOpponentHand(opponentId, revealedCard);

            _logger.LogInformation("Sent Priest reveal (Opponent: {OpponentId}, Card: {RevealedCardType}) to Player {PlayerId}.",
                opponentId, revealedCard.Type, requestingPlayerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Priest reveal to Player {PlayerId}.", requestingPlayerId);
            // Decide whether to re-throw or just log
        }
    }
    
    // --- Game Group Broadcast Methods ---

    public async Task BroadcastGuardGuessAsync(Guid gameId, Guid guesserId, Guid targetId, int guessedCardType,
        bool wasCorrect, CancellationToken cancellationToken)
    {
        string groupName = GetGameGroupName(gameId);
        try
        {
            await _hubContext.Clients.Group(groupName).PlayerGuessed(guesserId, targetId, guessedCardType, wasCorrect);
            _logger.LogInformation("Broadcast Guard guess result to group {GroupName} for Game {GameId}.", groupName,
                gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting Guard guess to group {GroupName} for Game {GameId}.", groupName,
                gameId);
        }
    }

    public async Task BroadcastBaronComparisonAsync(Guid gameId, Guid player1Id, int player1CardType, Guid player2Id,
        int player2CardType, Guid? loserId, CancellationToken cancellationToken)
    {
        string groupName = GetGameGroupName(gameId);
        try
        {
            await _hubContext.Clients.Group(groupName)
                .PlayersComparedHands(player1Id, player1CardType, player2Id, player2CardType, loserId);
            _logger.LogInformation("Broadcast Baron comparison result to group {GroupName} for Game {GameId}.",
                groupName, gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting Baron comparison to group {GroupName} for Game {GameId}.",
                groupName, gameId);
        }
    }

    public async Task BroadcastPlayerDiscardAsync(Guid gameId, Guid targetPlayerId, CardDto discardedCard,
        CancellationToken cancellationToken)
    {
        if (discardedCard == null) return; // Don't broadcast if nothing was discarded
        string groupName = GetGameGroupName(gameId);
        try
        {
            await _hubContext.Clients.Group(groupName).PlayerDiscarded(targetPlayerId, discardedCard);
            _logger.LogInformation("Broadcast Player discard result to group {GroupName} for Game {GameId}.", groupName,
                gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting Player discard to group {GroupName} for Game {GameId}.", groupName,
                gameId);
        }
    }

    public async Task BroadcastKingSwapAsync(Guid gameId, Guid player1Id, Guid player2Id,
        CancellationToken cancellationToken)
    {
        string groupName = GetGameGroupName(gameId);
        try
        {
            await _hubContext.Clients.Group(groupName).CardsSwapped(player1Id, player2Id);
            _logger.LogInformation("Broadcast King swap result to group {GroupName} for Game {GameId}.", groupName,
                gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting King swap to group {GroupName} for Game {GameId}.", groupName,
                gameId);
        }
    }
    
    public async Task BroadcastRoundWinnerAsync(Guid gameId, Guid? winnerId, string reason, Dictionary<Guid, int?> finalHands, CancellationToken cancellationToken)
    {
        string groupName = GetGameGroupName(gameId);
        try
        {
            await _hubContext.Clients.Group(groupName).RoundWinnerAnnounced(winnerId, reason, finalHands);
            _logger.LogInformation("Broadcast Round Winner announcement to group {GroupName} for Game {GameId}.", groupName, gameId);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error broadcasting Round Winner to group {GroupName} for Game {GameId}.", groupName, gameId); }
    }

    public async Task BroadcastGameWinnerAsync(Guid gameId, Guid winnerId, CancellationToken cancellationToken)
    {
        string groupName = GetGameGroupName(gameId);
        try
        {
            await _hubContext.Clients.Group(groupName).GameWinnerAnnounced(winnerId);
            _logger.LogInformation("Broadcast Game Winner announcement to group {GroupName} for Game {GameId}.", groupName, gameId);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error broadcasting Game Winner to group {GroupName} for Game {GameId}.", groupName, gameId); }
    }
    private static string GetGameGroupName(Guid gameId) => $"Game_{gameId}";
}