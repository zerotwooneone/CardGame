using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq;
using CardGame.Domain.Game.Event;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles CardEffectFizzled events to log the outcome.
/// </summary>
public class HandleCardEffectFizzledAndNotify : INotificationHandler<DomainEventNotification<CardEffectFizzled>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleCardEffectFizzledAndNotify> _logger;
    private readonly IGameRepository _gameRepository; 

    public HandleCardEffectFizzledAndNotify(IPlayerNotifier playerNotifier, 
                                          ILogger<HandleCardEffectFizzledAndNotify> logger,
                                          IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository)); 
    }

    public async Task Handle(DomainEventNotification<CardEffectFizzled> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling CardEffectFizzled domain event for Game {GameId}, Actor {ActorId}, Card {CardType}, Target {TargetId}, Reason: {Reason}. Primary log entry created in Game.cs.",
            domainEvent.GameId, domainEvent.ActorId, domainEvent.Rank.Name, domainEvent.TargetId, domainEvent.Reason);

        // The primary GameLogEntry for EffectFizzled (e.g., due to Handmaid) 
        // is created directly in the Game.cs methods (e.g., ExecuteKingEffect, ExecutePrinceEffect, etc.)
        // before the CardEffectFizzled domain event is raised.
        // This handler should not add a duplicate GameLogEntry.

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found when handling CardEffectFizzled event for notification purposes.", domainEvent.GameId);
            return;
        }

        // Logic for notifications can remain if _playerNotifier is used.
        // For example:
        // await _playerNotifier.BroadcastCardEffectFizzledAsync(
        //     domainEvent.GameId,
        //     domainEvent.ActorId,
        //     domainEvent.CardType.Value, // Ensure this aligns if CardType is a class/struct
        //     domainEvent.TargetId,
        //     domainEvent.Reason,
        //     cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("CardEffectFizzled event processed for Game {GameId}. Actor: {ActorId}, Card: {CardType}, Target: {TargetId}, Reason: {Reason}. Log entry was created in domain.",
            domainEvent.GameId, domainEvent.ActorId, domainEvent.Rank.Name, domainEvent.TargetId, domainEvent.Reason);

        // No game.AddLogEntry() call here anymore.
        // No _gameRepository.SaveAsync(game, ...) is needed here if the handler isn't modifying the game state related to the log.
    }
}