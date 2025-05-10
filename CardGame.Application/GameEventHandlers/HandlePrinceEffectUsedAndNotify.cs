using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq; 

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles PrinceEffectUsed events to log the discard outcome.
/// (Note: PlayerDrewCard handler handles sending the new hand state privately).
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

    public async Task Handle(DomainEventNotification<PrinceEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PrinceEffectUsed by {ActorPlayerId} on {TargetPlayerId} in Game {GameId}. Discarded: {DiscardedCardType}", 
            domainEvent.ActorPlayerId, domainEvent.TargetPlayerId, domainEvent.GameId, domainEvent.DiscardedCardType);

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

        // Format the message for the log entry
        // Assuming domainEvent.DiscardedCardType is CardGame.Domain.Types.CardType
        string discardedCardName = domainEvent.DiscardedCardType.ToString(); // Or a more friendly name
        string message;
        if (actorPlayer.Id == targetPlayer.Id)
        {
            message = $"{actorPlayer.Name} used Prince on themself, discarding {discardedCardName}.";
        }
        else
        {
            message = $"{actorPlayer.Name} used Prince on {targetPlayer.Name}, forcing them to discard {discardedCardName}.";
        }

        // if (domainEvent.WasPrincessDiscarded)
        // {
        //     message += $" {targetPlayer.Name} discarded the Princess and was knocked out!";
        // }

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.PrinceDiscard, // Corrected enum value
            actingPlayerId: domainEvent.ActorPlayerId, 
            actingPlayerName: actorPlayer.Name,
            targetPlayerId: domainEvent.TargetPlayerId, 
            targetPlayerName: targetPlayer.Name,
            message: message, // Use the formatted message
            isPrivate: false // Prince effect results are public
        );
        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);
    }
}