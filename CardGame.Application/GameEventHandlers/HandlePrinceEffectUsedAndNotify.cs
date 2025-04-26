using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles PrinceEffectUsed events to broadcast the discard outcome.
/// (Note: PlayerDrewCard handler handles sending the new hand state privately).
/// </summary>
public class HandlePrinceEffectUsedAndNotify : INotificationHandler<DomainEventNotification<PrinceEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandlePrinceEffectUsedAndNotify> _logger;
    // May need IGameRepository if CardDto needs more info than event provides (like ID)

    public HandlePrinceEffectUsedAndNotify(IPlayerNotifier playerNotifier,
        ILogger<HandlePrinceEffectUsedAndNotify> logger)
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<PrinceEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PrinceEffectUsed for Game {GameId}, Target {TargetPlayerId}.", domainEvent.GameId,
            domainEvent.TargetPlayerId);

        var discardedCardDto = new CardDto
        {
            Id = domainEvent.DiscardedCardId, // If event included ID
            Type = domainEvent.DiscardedCardType.Value,
        };

        await _playerNotifier.BroadcastPlayerDiscardAsync(
            domainEvent.GameId,
            domainEvent.TargetPlayerId,
            discardedCardDto,
            cancellationToken).ConfigureAwait(false);
    }
}