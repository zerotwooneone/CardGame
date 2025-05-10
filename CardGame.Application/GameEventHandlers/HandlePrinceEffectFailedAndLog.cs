using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the PrinceEffectFailed domain event. Logging is now centralized in Game.ExecutePrinceEffect.
/// This handler is now primarily for debug logging or highly specific notifications if any.
/// </summary>
public class HandlePrinceEffectFailedAndLog : INotificationHandler<DomainEventNotification<PrinceEffectFailed>>
{
    private readonly ILogger<HandlePrinceEffectFailedAndLog> _logger;

    public HandlePrinceEffectFailedAndLog(
        ILogger<HandlePrinceEffectFailedAndLog> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(DomainEventNotification<PrinceEffectFailed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PrinceEffectFailed event for Game {GameId}, TargetPlayerId: {TargetPlayerId}, Reason: {Reason}. Primary logging done in Game.ExecutePrinceEffect.",
            domainEvent.GameId, domainEvent.TargetPlayerId, domainEvent.Reason);

        // All substantive logging (creating GameLogEntry) is now done within Game.ExecutePrinceEffect.
        // This handler can be kept for debug purposes or if any specific, non-log-related action
        // needs to be taken upon PrinceEffectFailed that isn't covered by general game state updates.

        // No SaveAsync needed as game state changes (if any that led to failure) and logs are in Game aggregate.
        return Task.CompletedTask;
    }
}
