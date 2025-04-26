using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles BaronComparisonResult events to broadcast the outcome.
/// </summary>
public class HandleBaronComparisonResultAndNotify : INotificationHandler<DomainEventNotification<BaronComparisonResult>>
{
    private readonly IPlayerNotifier _playerNotifier;
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
        _logger.LogDebug("Handling BaronComparisonResult for Game {GameId}.", domainEvent.GameId);

        await _playerNotifier.BroadcastBaronComparisonAsync(
            domainEvent.GameId,
            domainEvent.Player1Id,
            domainEvent.Player1Card.Value, 
            domainEvent.Player2Id,
            domainEvent.Player2Card.Value, 
            domainEvent.LoserPlayerId,
            cancellationToken).ConfigureAwait(false);
    }
}