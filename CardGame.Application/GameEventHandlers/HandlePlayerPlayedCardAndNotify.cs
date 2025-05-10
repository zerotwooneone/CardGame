using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the generic PlayerPlayedCard event to create a public log entry.
/// </summary>
public class HandlePlayerPlayedCardAndNotify : INotificationHandler<DomainEventNotification<PlayerPlayedCard>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandlePlayerPlayedCardAndNotify> _logger;

    public HandlePlayerPlayedCardAndNotify(
        IGameRepository gameRepository,
        ILogger<HandlePlayerPlayedCardAndNotify> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PlayerPlayedCard> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerPlayedCard event for Game {GameId}. Player {PlayerId} played {CardType}", 
            domainEvent.GameId, domainEvent.PlayerId, domainEvent.PlayedCard.Name);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PlayerPlayedCard event.", domainEvent.GameId);
            return;
        }

        var actingPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);
        if (actingPlayer == null)
        {
            _logger.LogWarning("Acting player {PlayerId} not found in Game {GameId} for PlayerPlayedCard event.", domainEvent.PlayerId, domainEvent.GameId);
            return;
        }

        string actingPlayerName = actingPlayer.Name;
        string playedCardName = domainEvent.PlayedCard.Name;
        string message;
        string? targetPlayerName = null;

        if (domainEvent.TargetPlayerId.HasValue)
        {
            var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetPlayerId.Value);
            if (targetPlayer != null)
            {
                targetPlayerName = targetPlayer.Name;
                message = $"{actingPlayerName} played {playedCardName} targeting {targetPlayerName}.";
                if (domainEvent.PlayedCard == CardType.Guard && domainEvent.GuessedCardType != null)
                {
                    message += $" They guessed {domainEvent.GuessedCardType.Name}.";
                }
            }
            else
            {
                _logger.LogWarning("Target player {TargetPlayerId} not found in Game {GameId} for PlayerPlayedCard event.", domainEvent.TargetPlayerId.Value, domainEvent.GameId);
                message = $"{actingPlayerName} played {playedCardName} targeting an unknown player."; // Fallback
            }
        }
        else
        {
            message = $"{actingPlayerName} played {playedCardName}.";
        }

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.CardPlayed,
            actingPlayerId: domainEvent.PlayerId,
            actingPlayerName: actingPlayerName,
            isPrivate: false,
            message: message,
            targetPlayerId: domainEvent.TargetPlayerId,
            targetPlayerName: targetPlayerName, 
            playedCardType: domainEvent.PlayedCard,
            guessedCardType: domainEvent.GuessedCardType // For Guard, this provides early guess info
        );

        game.AddLogEntry(logEntry);
        // This handler only adds the log. Saving is done by the PlayCardCommandHandler after all events are processed for a play.
        // However, if other handlers for specific card effects also call SaveAsync, this might lead to multiple saves.
        // The current design in PlayCardCommandHandler is to save once after game.PlayCard completes and before publishing events.
        // So, this handler should NOT call SaveAsync(). The log will be part of the game state saved by PlayCardCommandHandler.

        await Task.CompletedTask; // No async repo calls here needed for logging itself
    }
}
