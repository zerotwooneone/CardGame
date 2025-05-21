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
/// Handles the PlayerEliminated domain event. The primary GameLogEntry is created in Game.EliminatePlayer.
/// This handler is for debug logging and potential future notifications.
/// </summary>
public class HandlePlayerEliminatedAndNotify : INotificationHandler<DomainEventNotification<PlayerEliminated>>
{
    private readonly ILogger<HandlePlayerEliminatedAndNotify> _logger;

    public HandlePlayerEliminatedAndNotify(
        ILogger<HandlePlayerEliminatedAndNotify> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(DomainEventNotification<PlayerEliminated> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerEliminated event for Game {GameId}, Player {PlayerId}. Reason: {Reason}", 
            domainEvent.GameId, domainEvent.PlayerId, domainEvent.Reason);

 

        return Task.CompletedTask;
    }
}
