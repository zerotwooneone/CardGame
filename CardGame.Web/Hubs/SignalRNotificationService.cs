using CardGame.Application.GameEventHandlers;
using Microsoft.AspNetCore.SignalR;
using CardGame.Web.Hubs; 

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(IHubContext<NotificationHub, INotificationClient> hubContext, ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendGameInvitationAsync(Guid invitedPlayerId, Guid gameId, string creatorUsername, CancellationToken cancellationToken)
    {
        try
        {
            await _hubContext.Clients
                .User(invitedPlayerId.ToString()) // Target specific user via their PlayerId
                .ReceiveGameInvitation(gameId, creatorUsername); // Invoke client method

            _logger.LogInformation("Sent game invitation for Game {GameId} to Player {PlayerId}", gameId, invitedPlayerId);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error sending game invitation to Player {PlayerId} for Game {GameId}", invitedPlayerId, gameId);
            // Decide whether to re-throw or just log
        }
    }
}