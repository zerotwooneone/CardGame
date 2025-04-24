using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Web.Hubs;

public class SignalRGameStateBroadcaster : IGameStateBroadcaster
{
    private readonly IHubContext<GameHub, IGameClient> _hubContext;
    private readonly ILogger<SignalRGameStateBroadcaster> _logger;

    public SignalRGameStateBroadcaster(IHubContext<GameHub, IGameClient> hubContext, ILogger<SignalRGameStateBroadcaster> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastSpectatorStateAsync(Guid gameId, SpectatorGameStateDto gameState, CancellationToken cancellationToken)
    {
        string groupName = $"Game_{gameId}";
        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .UpdateSpectatorGameState(gameState); // Call client method

            _logger.LogInformation("Broadcast spectator state for Game {GameId} to group {GroupName}", gameId, groupName);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting spectator state for Game {GameId} to group {GroupName}", gameId, groupName);
            // Decide whether to re-throw or just log
        }
    }
}