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
    
    
    public async Task BroadcastRoundSummaryAsync(Guid gameId, RoundEndSummaryDto summaryData, CancellationToken cancellationToken) // Changed signature
    {
        string groupName = GetGameGroupName(gameId);
        try
        {
            // Call the updated client method with the new DTO
            await _hubContext.Clients.Group(groupName).ShowRoundSummary(summaryData);
            _logger.LogInformation("Broadcast Round Summary to group {GroupName} for Game {GameId}.", groupName, gameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting Round Summary to group {GroupName} for Game {GameId}.", groupName, gameId);
        }
    }

    public async Task SendPriestRevealAsync(Guid priestPlayerId, Guid targetPlayerId, string targetPlayerName, CardDto revealedCard, CancellationToken cancellationToken)
    {
        if (priestPlayerId == Guid.Empty || revealedCard == null)
        {
            _logger.LogWarning("SendPriestRevealAsync called with invalid arguments. PriestPlayerId: {PriestPlayerId}, RevealedCard: {RevealedCard}", priestPlayerId, revealedCard);
            return;
        }

        string userId = priestPlayerId.ToString(); // SignalR targets users by string ID
        try
        {
            await _hubContext.Clients
                .User(userId)
                .ReceivePriestReveal(targetPlayerId, targetPlayerName, revealedCard);

            _logger.LogInformation("Sent Priest reveal to Player {PriestPlayerId} about Target {TargetPlayerId}'s card (Rank: {RevealedCardRank}).", 
                priestPlayerId, targetPlayerId, revealedCard.Rank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Priest reveal to Player {PriestPlayerId}.", priestPlayerId);
        }
    }
}