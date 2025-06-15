using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the PrinceEffectUsed domain event. The primary GameLogEntry for the Prince effect (discard and subsequent draw)
/// is created in Game.ExecutePrinceEffect. This handler is for debug logging and potential future notifications related to the Prince effect.
/// (Note: PlayerDrewCard handler handles sending the new hand state privately if a card is drawn).
/// </summary>
public class HandlePrinceEffectUsedAndNotify : INotificationHandler<DomainEventNotification<PrinceEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandlePrinceEffectUsedAndNotify> _logger;
    private readonly IGameRepository _gameRepository; 

    public HandlePrinceEffectUsedAndNotify(IPlayerNotifier playerNotifier,
        ILogger<HandlePrinceEffectUsedAndNotify> logger,
        IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
        _gameRepository = gameRepository; 
    }

    public Task Handle(DomainEventNotification<PrinceEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PrinceEffectUsed by {ActorPlayerId} on {TargetPlayerId} in Game {GameId}. Discarded: {DiscardedCardType}", 
            domainEvent.ActorPlayerId, domainEvent.TargetPlayerId, domainEvent.GameId, domainEvent.DiscardedCard.Rank);

 
        return Task.CompletedTask;
    }
}