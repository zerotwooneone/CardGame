using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles BaronComparisonResult events. The primary logging is now done in Game.ExecuteBaronEffect.
/// This handler is now mainly for debug logging or specific notifications if any.
/// </summary>
public class HandleBaronComparisonResultAndNotify : INotificationHandler<DomainEventNotification<BaronComparisonResult>>
{
    private readonly IPlayerNotifier _playerNotifier; // Kept in case of specific Baron notifications
    private readonly ILogger<HandleBaronComparisonResultAndNotify> _logger;

    public HandleBaronComparisonResultAndNotify(IPlayerNotifier playerNotifier,
        ILogger<HandleBaronComparisonResultAndNotify> logger)
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<BaronComparisonResult> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling BaronComparisonResult for Game {GameId}. Player1: {Player1Id} (Card: {Player1Card}), Player2: {Player2Id} (Card: {Player2Card}), Loser: {LoserPlayerId}. Log entry created in Game.cs.", 
            domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player1Card, domainEvent.Player2Id, domainEvent.Player2Card, domainEvent.LoserPlayerId);

        
        return Task.CompletedTask; // Placeholder if no async notifier call is made
    }
}