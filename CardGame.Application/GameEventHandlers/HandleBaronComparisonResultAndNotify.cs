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

    public async Task Handle(DomainEventNotification<BaronComparisonResult> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling BaronComparisonResult for Game {GameId}. Player1: {Player1Id} (Card: {Player1Card}), Player2: {Player2Id} (Card: {Player2Card}), Loser: {LoserPlayerId}. Log entry created in Game.cs.", 
            domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player1Card, domainEvent.Player2Id, domainEvent.Player2Card, domainEvent.LoserPlayerId);

        // All detailed logging and game state changes (elimination) are handled within Game.ExecuteBaronEffect.
        // The GameLogEntry of type BaronCompare is created there.

        // The previous version had a commented-out _playerNotifier.BroadcastBaronComparisonAsync.
        // If such specific, direct notification is still required (beyond what general game state updates provide),
        // it would be called here. Example:
        // await _playerNotifier.NotifyBaronOutcomeAsync(
        //     domainEvent.GameId,
        //     domainEvent.Player1Id,
        //     domainEvent.Player1Card,
        //     domainEvent.Player2Id,
        //     domainEvent.Player2Card,
        //     domainEvent.LoserPlayerId,
        //     cancellationToken
        // ).ConfigureAwait(false);

        // For now, assuming general game state updates are sufficient based on prior comments.
        // If no specific notification is needed, this handler might only perform logging.
        await Task.CompletedTask; // Placeholder if no async notifier call is made
    }
}