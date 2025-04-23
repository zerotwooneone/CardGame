namespace CardGame.Application.GameEventHandlers;

public interface INotificationService
{
    Task SendGameInvitationAsync(Guid invitedPlayerId, Guid gameId, string creatorUsername, CancellationToken cancellationToken);
    // Add other notification methods as needed
}