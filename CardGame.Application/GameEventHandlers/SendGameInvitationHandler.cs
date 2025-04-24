using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped GameCreated domain event to send invitations to players
/// via an abstracted notification service.
/// </summary>
public class SendGameInvitationsHandler : INotificationHandler<DomainEventNotification<GameCreated>>
{
    private readonly INotificationService _notificationService; // Use the abstraction
    private readonly IUserRepository _userRepository; // Keep for username lookup if needed
    private readonly ILogger<SendGameInvitationsHandler> _logger; // Add logger

    public SendGameInvitationsHandler(
        INotificationService notificationService,
        IUserRepository userRepository,
        ILogger<SendGameInvitationsHandler> logger) // Inject logger
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Handle method now accepts the wrapper notification
    public async Task Handle(DomainEventNotification<GameCreated> notification, CancellationToken cancellationToken)
    {
        // Access the original domain event from the wrapper
        var domainEvent = notification.DomainEvent;

        // Find the creator's username
        // Assumption: GameCreated event includes PlayerInfo which has Id and Name.
        // A more robust approach might involve passing the CreatorPlayerId explicitly
        // in the GameCreated event and looking up their name here if needed.
        var creatorInfo = domainEvent.Players.FirstOrDefault(); // Simplified assumption
        if (creatorInfo == null)
        {
            _logger.LogWarning("Could not determine creator from GameCreated event for Game {GameId}.", domainEvent.GameId);
            return; // Cannot proceed without creator info
        }
        var creatorUsername = creatorInfo.Name;

        _logger.LogInformation("Handling GameCreated event for Game {GameId}. Sending invitations...", domainEvent.GameId);

        // Send invitation to all players EXCEPT the creator
        foreach (var playerInfo in domainEvent.Players)
        {
            if (playerInfo.Id != creatorInfo.Id) // Don't send to creator
            {
                try
                {
                    // Use the abstracted service to send the invitation
                    await _notificationService.SendGameInvitationAsync(
                        invitedPlayerId: playerInfo.Id,
                        gameId: domainEvent.GameId,
                        creatorUsername: creatorUsername,
                        cancellationToken: cancellationToken).ConfigureAwait(false); // Pass CancellationToken

                     _logger.LogInformation("Sent game invitation for Game {GameId} to Player {PlayerId}", domainEvent.GameId, playerInfo.Id);
                }
                catch (Exception ex)
                {
                    // Log error sending notification (user might not be connected, service unavailable, etc.)
                    _logger.LogError(ex, "Error sending game invitation to Player {PlayerId} for Game {GameId}", playerInfo.Id, domainEvent.GameId);
                    // Decide whether to continue processing other players or stop/rethrow
                }
            }
        }
    }
}