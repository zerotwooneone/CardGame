using CardGame.Application.Common.Notifications;
using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the HandmaidProtectionSet domain event to log when a player becomes protected.
/// </summary>
public class HandleHandmaidProtectionSetAndLog : INotificationHandler<DomainEventNotification<HandmaidProtectionSet>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleHandmaidProtectionSetAndLog> _logger;

    public HandleHandmaidProtectionSetAndLog(
        IGameRepository gameRepository,
        ILogger<HandleHandmaidProtectionSetAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<HandmaidProtectionSet> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling HandmaidProtectionSet for Player {PlayerId} in Game {GameId}",
            domainEvent.PlayerId, domainEvent.GameId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for HandmaidProtectionSet event.", domainEvent.GameId);
            return;
        }

        var protectedPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);
        if (protectedPlayer == null)
        {
            _logger.LogWarning("Player {PlayerId} not found in Game {GameId} for HandmaidProtectionSet event.",
                domainEvent.PlayerId, domainEvent.GameId);
            return;
        }

        var logMessage = $"{protectedPlayer.Name} is protected by the Handmaid.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.HandmaidProtection,
            actingPlayerId: protectedPlayer.Id,
            actingPlayerName: protectedPlayer.Name,
            message: logMessage,
            isPrivate: false // Handmaid protection is public knowledge
        );

        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged HandmaidProtectionSet for Player {PlayerName} ({PlayerId}) in Game {GameId}",
            protectedPlayer.Name, protectedPlayer.Id, game.Id);
    }
}
