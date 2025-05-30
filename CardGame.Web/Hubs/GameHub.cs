using CardGame.Application.DTOs;
using CardGame.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Web.Hubs;

/// <summary>
/// SignalR hub for handling real-time communication related to specific game instances.
/// Manages groups based on Game IDs for broadcasting game state updates.
/// </summary>
public class GameHub : Hub<IGameClient> // Specify client interface for strong typing
{
    private readonly ILogger<GameHub> _logger;
    private readonly IMediator _mediator;

    public GameHub(ILogger<GameHub> logger,
        IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator;
    }

    // --- Client-to-Server Methods ---

    /// <summary>
    /// Allows a client (player or spectator) to join the group for a specific game
    /// to receive real-time updates for that game.
    /// </summary>
    /// <param name="gameId">The ID of the game to join.</param>
    public async Task JoinGameGroup(Guid gameId)
    {
        if (gameId == Guid.Empty) return;

        string groupName = GetGameGroupName(gameId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // Log joining action
        var userId = Context.UserIdentifier ?? "anonymous"; // Use PlayerId if available
        _logger.LogInformation("Client {ConnectionId} (User: {UserId}) joined game group: {GroupName}", Context.ConnectionId, userId, groupName);

        // Send the current state immediately to the joining client
        var gameState = await _mediator.Send(new GetSpectatorGameStateQuery(gameId));
        if (gameState != null) {
            await Clients.Client(Context.ConnectionId).UpdateSpectatorGameState(gameState);
        }
    }

    /// <summary>
    /// Allows a client to leave the group for a specific game when they navigate away
    /// or are no longer interested in updates for that game.
    /// </summary>
    /// <param name="gameId">The ID of the game group to leave.</param>
    public async Task LeaveGameGroup(Guid gameId)
    {
         if (gameId == Guid.Empty) return;

        string groupName = GetGameGroupName(gameId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        // Log leaving action
        var userId = Context.UserIdentifier ?? "anonymous";
        _logger.LogInformation("Client {ConnectionId} (User: {UserId}) left game group: {GroupName}", Context.ConnectionId, userId, groupName);
    }

    // --- Server-to-Client Methods (Defined in IGameClient interface) ---
    // The server will call methods on clients like:
    // - UpdateSpectatorGameState(SpectatorGameStateDto gameState)

    // --- Hub Lifecycle Methods (Optional Overrides) ---
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier ?? "anonymous";
        _logger.LogInformation("GameHub: Client connected: {ConnectionId} (User: {UserId})", Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier ?? "anonymous";
        if (exception == null)
        {
            _logger.LogInformation("GameHub: Client disconnected: {ConnectionId} (User: {UserId})", Context.ConnectionId, userId);
        }
        else
        {
            _logger.LogWarning(exception, "GameHub: Client disconnected with error: {ConnectionId} (User: {UserId})", Context.ConnectionId, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetGameGroupName(Guid gameId) => $"Game_{gameId}";

}

/// <summary>
/// Interface defining the client methods that the GameHub can invoke.
/// Useful for strongly-typed hub contexts.
/// </summary>
public interface IGameClient
{
    /// <summary>
    /// Sends the updated public game state to clients in the game group.
    /// </summary>
    /// <param name="gameState">The spectator game state DTO.</param>
    Task UpdateSpectatorGameState(SpectatorGameStateDto gameState);
    
    /// <summary>
    /// Sends the updated private hand state directly to a specific player.
    /// </summary>
    /// <param name="currentHand">A list of Card DTOs representing the player's current hand.</param>
    Task UpdatePlayerHand(List<CardDto> currentHand);
    
    /// <summary>
    /// Announces the winner of the game to the game group.
    /// </summary>
    /// <param name="winnerId">The ID of the player who won the game.</param>
    Task GameWinnerAnnounced(Guid winnerId);
    
    Task ShowRoundSummary(RoundEndSummaryDto summaryData);

    /// <summary>
    /// Sends information about a card revealed by a Priest effect to the client of the player who played the Priest.
    /// </summary>
    /// <param name="targetPlayerId">The ID of the player whose card was revealed.</param>
    /// <param name="targetPlayerName">The name of the player whose card was revealed.</param>
    /// <param name="revealedCard">The card that was revealed from the target player's hand.</param>
    Task ReceivePriestReveal(Guid targetPlayerId, string targetPlayerName, CardDto revealedCard);
}