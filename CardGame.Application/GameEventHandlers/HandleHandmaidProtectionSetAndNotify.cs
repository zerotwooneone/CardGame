using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the HandmaidProtectionSet domain event. The primary GameLogEntry is created in Game.ExecuteHandmaidEffect.
/// This handler is for debug logging and potential future notifications.
/// </summary>
public class HandleHandmaidProtectionSetAndNotify : INotificationHandler<DomainEventNotification<HandmaidProtectionSet>>
{
    private readonly ILogger<HandleHandmaidProtectionSetAndNotify> _logger;

    public HandleHandmaidProtectionSetAndNotify(
        ILogger<HandleHandmaidProtectionSetAndNotify> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(DomainEventNotification<HandmaidProtectionSet> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling HandmaidProtectionSet event for Game {GameId}, Player {PlayerId}", 
            domainEvent.GameId, domainEvent.PlayerId);

        return Task.CompletedTask;
    }
}
