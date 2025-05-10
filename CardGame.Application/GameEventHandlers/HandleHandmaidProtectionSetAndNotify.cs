using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the HandmaidProtectionSet event to create a log entry announcing the protection.
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
