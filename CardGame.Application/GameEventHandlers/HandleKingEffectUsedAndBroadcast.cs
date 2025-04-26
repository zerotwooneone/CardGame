using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles KingEffectUsed events to broadcast the swap outcome.
/// (Note: HandleKingEffectUsedAndNotify in hand_update_handlers handles sending private hand updates).
/// </summary>
public class HandleKingEffectUsedAndBroadcast : INotificationHandler<DomainEventNotification<KingEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleKingEffectUsedAndBroadcast> _logger;

    public HandleKingEffectUsedAndBroadcast(IPlayerNotifier playerNotifier,
        ILogger<HandleKingEffectUsedAndBroadcast> logger)
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<KingEffectUsed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling KingEffectUsed for Game {GameId} between {Player1Id} and {Player2Id}.",
            domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player2Id);

        await _playerNotifier.BroadcastKingSwapAsync(
            domainEvent.GameId,
            domainEvent.Player1Id,
            domainEvent.Player2Id,
            cancellationToken).ConfigureAwait(false);
    }
}