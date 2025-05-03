using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles CardEffectFizzled events to broadcast the outcome.
/// </summary>
public class HandleCardEffectFizzledAndNotify : INotificationHandler<DomainEventNotification<CardEffectFizzled>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleCardEffectFizzledAndNotify> _logger;

    public HandleCardEffectFizzledAndNotify(IPlayerNotifier playerNotifier, ILogger<HandleCardEffectFizzledAndNotify> logger)
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<CardEffectFizzled> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling CardEffectFizzled for Game {GameId}, Actor {ActorId}, Card {CardType}, Target {TargetId}.",
            domainEvent.GameId, domainEvent.ActorId, domainEvent.CardType.Name, domainEvent.TargetId);

        // Call the broadcast method on the notifier service
        await _playerNotifier.BroadcastCardEffectFizzledAsync(
            domainEvent.GameId,
            domainEvent.ActorId,
            domainEvent.CardType.Value,
            domainEvent.TargetId,
            domainEvent.Reason,
            cancellationToken).ConfigureAwait(false);
    }
}