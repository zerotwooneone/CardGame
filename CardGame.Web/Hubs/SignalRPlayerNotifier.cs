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
}