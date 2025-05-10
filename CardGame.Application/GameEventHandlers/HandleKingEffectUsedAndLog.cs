using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the KingEffectUsed domain event. Logging is now centralized in Game.ExecuteKingEffect.
/// This handler is now primarily for debug logging or highly specific notifications.
/// </summary>
public class HandleKingEffectUsedAndLog : INotificationHandler<DomainEventNotification<KingEffectUsed>>
{
    private readonly ILogger<HandleKingEffectUsedAndLog> _logger;

    public HandleKingEffectUsedAndLog(
        ILogger<HandleKingEffectUsedAndLog> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(DomainEventNotification<KingEffectUsed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling KingEffectUsed event for Game {GameId}, Player1: {Player1Id}, Player2: {Player2Id}. Primary logging in Game.ExecuteKingEffect.",
            domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player2Id);

        // All substantive logging (GameLogEntry creation) is now in Game.ExecuteKingEffect.
        // This handler can be kept for debug purposes or for any specific non-log-related actions.

        // No SaveAsync needed as game state changes (hand swap) and logs are within the Game aggregate.
        return Task.CompletedTask;
    }
}
