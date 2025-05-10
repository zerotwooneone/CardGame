using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq; 

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
        _logger.LogDebug("Handling CardEffectFizzled for Game {GameId}, Actor {ActorId}, Card {CardType}, Target {TargetId}, Reason: {Reason}.",
            domainEvent.GameId, domainEvent.ActorId, domainEvent.CardType.Name, domainEvent.TargetId, domainEvent.Reason);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for CardEffectFizzled event.", domainEvent.GameId);
            return;
        }

        var actorPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.ActorId);
        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetId);

        if (actorPlayer == null)
        {
            _logger.LogWarning("Actor Player not found for CardEffectFizzled event in Game {GameId}. ActorId: {ActorId}", 
                domainEvent.GameId, domainEvent.ActorId);
            return;
        }
        // TargetPlayer can be null if the effect didn't target a specific player or the target was invalid.

        // Format the message for the log entry
        // Assuming domainEvent.CardType is CardGame.Domain.Types.CardType
        string cardName = domainEvent.CardType.ToString(); // Or a more friendly name
        string message;
        if (targetPlayer != null)
        {
            message = $"{actorPlayer.Name}'s {cardName} effect against {targetPlayer.Name} fizzled. Reason: {domainEvent.Reason}.";
        }
        else 
        {
            message = $"{actorPlayer.Name}'s {cardName} effect against Player ID {domainEvent.TargetId} fizzled. Reason: {domainEvent.Reason} (Target player not found).";
        }

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.EffectFizzled, // Corrected enum value
            actingPlayerId: domainEvent.ActorId,
            actingPlayerName: actorPlayer.Name,
            message: message, // Use the formatted message
            isPrivate: false // Fizzle events are public
        );
        game.AddLogEntry(logEntry);

        // await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        // // Call the broadcast method on the notifier service
        // await _playerNotifier.BroadcastCardEffectFizzledAsync(
        //     domainEvent.GameId,
        //     domainEvent.ActorId,
        //     domainEvent.CardType.Value,
        //     domainEvent.TargetId,
        //     domainEvent.Reason,
        //     cancellationToken).ConfigureAwait(false);
    }
}