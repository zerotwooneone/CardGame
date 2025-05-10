using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the PlayerPlayedCountess domain event to log when a player plays/discards the Countess.
/// </summary>
public class HandlePlayerPlayedCountessAndLog : INotificationHandler<DomainEventNotification<PlayerPlayedCountess>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandlePlayerPlayedCountessAndLog> _logger;

    public HandlePlayerPlayedCountessAndLog(
        IGameRepository gameRepository,
        ILogger<HandlePlayerPlayedCountessAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PlayerPlayedCountess> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerPlayedCountess: Player {PlayerId} in Game {GameId}",
            domainEvent.PlayerId, domainEvent.GameId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PlayerPlayedCountess event.", domainEvent.GameId);
            return;
        }

        var player = game.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);
        if (player == null)
        {
            _logger.LogWarning("Player {PlayerId} not found for PlayerPlayedCountess event in Game {GameId}.",
                domainEvent.PlayerId, domainEvent.GameId);
            return;
        }
        
        var logMessage = $"{player.Name} played the Countess.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.CountessDiscard,
            actingPlayerId: player.Id,
            actingPlayerName: player.Name,
            message: logMessage,
            isPrivate: false // Countess play is public
        );

        game.AddLogEntry(logEntry);
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged PlayerPlayedCountess: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
