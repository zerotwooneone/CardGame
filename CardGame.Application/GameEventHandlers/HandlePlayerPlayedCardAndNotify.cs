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
/// Handles the generic PlayerPlayedCard event to create a public log entry.
/// </summary>
public class HandlePlayerPlayedCardAndNotify : INotificationHandler<DomainEventNotification<PlayerPlayedCard>>
{
    private readonly ILogger<HandlePlayerPlayedCardAndNotify> _logger;

    public HandlePlayerPlayedCardAndNotify(
        ILogger<HandlePlayerPlayedCardAndNotify> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(DomainEventNotification<PlayerPlayedCard> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerPlayedCard event for Game {GameId}. Player {PlayerId} played {CardType}. Logging is now done in Game.PlayCard.", 
            domainEvent.GameId, domainEvent.PlayerId, domainEvent.PlayedCard.ToString());
        
        return Task.CompletedTask;
    }
}
