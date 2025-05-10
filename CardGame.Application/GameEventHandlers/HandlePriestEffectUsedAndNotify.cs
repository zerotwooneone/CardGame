using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain; 
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

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

        // The public log entry for playing the Priest is now handled by HandlePlayerPlayedCardAndNotify.
        // Create and add private game log entry for the revealed card (for the acting player)
        string revealedCardName = domainEvent.RevealedCardType.ToString(); // Or a more friendly name
        var privateLogMessage = $"You used Priest on {targetPlayerName} and saw they have a {revealedCardName}.";
        var privateLogEntry = new GameLogEntry(
            eventType: GameLogEventType.PriestEffect, // Specific event for the Priest's effect
            actingPlayerId: domainEvent.PriestPlayerId,
            actingPlayerName: actingPlayerName,
            isPrivate: true, // This log is private to the acting player
            message: privateLogMessage,
            targetPlayerId: domainEvent.TargetPlayerId, // Still relevant to know who was targeted
            targetPlayerName: targetPlayerName,
            // PlayedCardType is contextually known (Priest), but can be included for completeness in structured data
            playedCardType: CardType.Priest, // The card that was played to trigger this effect
            revealedCardId: domainEvent.RevealedCardId, // The ID of the card that was revealed
            revealedByPriestCardType: domainEvent.RevealedCardType // The type of card that was revealed
        );
        game.AddLogEntry(privateLogEntry);

        // No SaveAsync here, PlayCardCommandHandler handles saving before event publishing.
        await Task.CompletedTask;
    }
}