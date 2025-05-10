using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the PrinceEffectUsed domain event to log when a player is forced to discard by Prince.
/// </summary>
public class HandlePrinceEffectUsedAndLog : INotificationHandler<DomainEventNotification<PrinceEffectUsed>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandlePrinceEffectUsedAndLog> _logger;

    public HandlePrinceEffectUsedAndLog(
        IGameRepository gameRepository,
        ILogger<HandlePrinceEffectUsedAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PrinceEffectUsed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PrinceEffectUsed: Actor {ActorId} targeted {TargetId}, who discarded {DiscardedCardType} ({DiscardedCardId}) in Game {GameId}",
            domainEvent.ActorPlayerId, domainEvent.TargetPlayerId, domainEvent.DiscardedCardType.Name, domainEvent.DiscardedCardId, domainEvent.GameId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PrinceEffectUsed event.", domainEvent.GameId);
            return;
        }

        var actorPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.ActorPlayerId);
        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetPlayerId);

        if (actorPlayer == null || targetPlayer == null)
        {
            _logger.LogWarning("Actor or Target Player not found for PrinceEffectUsed event in Game {GameId}. ActorId: {ActorId}, TargetId: {TargetId}",
                domainEvent.GameId, domainEvent.ActorPlayerId, domainEvent.TargetPlayerId);
            return;
        }
        
        var logMessage = $"{actorPlayer.Name} used Prince, forcing {targetPlayer.Name} to discard their {domainEvent.DiscardedCardType.Name}.";
        if (domainEvent.DiscardedCardType == CardType.Princess)
        {
            logMessage += " (Princess!)"; // The PlayerEliminated log will formalize the knockout.
        }

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.PrinceDiscard,
            actingPlayerId: actorPlayer.Id, 
            actingPlayerName: actorPlayer.Name,
            targetPlayerId: targetPlayer.Id,
            targetPlayerName: targetPlayer.Name,
            // Store the discarded card info directly, might be useful for UI to highlight it
            revealedCardType: domainEvent.DiscardedCardType, // 'Revealed' is a bit of a misnomer, but it fits existing structure for card type
            revealedCardId: domainEvent.DiscardedCardId,     // Same for card ID
            isPrivate: false // Prince discards are public
        );

        game.AddLogEntry(logEntry);
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged PrinceEffectUsed: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
