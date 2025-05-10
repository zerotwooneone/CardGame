using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain; 
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped PriestEffectUsed domain event to send the revealed
/// card information (ID and Type) directly to the player who played the Priest,
/// and logs the event to the game log.
/// </summary>
public class HandlePriestEffectUsedAndNotify : INotificationHandler<DomainEventNotification<PriestEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly IGameRepository _gameRepository; 
    private readonly ILogger<HandlePriestEffectUsedAndNotify> _logger;

    public HandlePriestEffectUsedAndNotify(
        IPlayerNotifier playerNotifier,
        IGameRepository gameRepository, 
        ILogger<HandlePriestEffectUsedAndNotify> logger)
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository)); 
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PriestEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug(
            "Handling PriestEffectUsed by Player {PriestPlayerId} targeting {TargetPlayerId} in Game {GameId}. Revealed CardId: {RevealedCardId}",
            domainEvent.PriestPlayerId, domainEvent.TargetPlayerId, domainEvent.GameId, domainEvent.RevealedCardId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PriestEffectUsed event.", domainEvent.GameId);
            return;
        }

        var actingPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.PriestPlayerId);
        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetPlayerId);

        if (actingPlayer == null || targetPlayer == null)
        {
            _logger.LogWarning("Acting or Target Player not found for PriestEffectUsed event in Game {GameId}. ActingPlayerId: {ActingPlayerId}, TargetPlayerId: {TargetPlayerId}", 
                domainEvent.GameId, domainEvent.PriestPlayerId, domainEvent.TargetPlayerId);
            return;
        }

        var actingPlayerName = actingPlayer.Name;
        var targetPlayerName = targetPlayer.Name;

        // Create and add game log entry
        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.PriestEffect,
            actingPlayerId: domainEvent.PriestPlayerId,
            actingPlayerName: actingPlayerName,
            targetPlayerId: domainEvent.TargetPlayerId,
            targetPlayerName: targetPlayerName,
            revealedCardId: domainEvent.RevealedCardId,
            revealedCardType: domainEvent.RevealedCardType,
            isPrivate: true // Priest reveal is private to the acting player
        );
        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);
    }
}