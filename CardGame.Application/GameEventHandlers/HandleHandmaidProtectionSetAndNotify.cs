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
/// Handles the HandmaidProtectionSet event to create a log entry announcing the protection.
/// </summary>
public class HandleHandmaidProtectionSetAndNotify : INotificationHandler<DomainEventNotification<HandmaidProtectionSet>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleHandmaidProtectionSetAndNotify> _logger;

    public HandleHandmaidProtectionSetAndNotify(
        IGameRepository gameRepository,
        ILogger<HandleHandmaidProtectionSetAndNotify> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<HandmaidProtectionSet> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling HandmaidProtectionSet event for Game {GameId}, Player {PlayerId}", 
            domainEvent.GameId, domainEvent.PlayerId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for HandmaidProtectionSet event.", domainEvent.GameId);
            return;
        }

        var protectedPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);
        if (protectedPlayer == null)
        {
            _logger.LogWarning("Protected player {PlayerId} not found in Game {GameId} for HandmaidProtectionSet event.", 
                domainEvent.PlayerId, domainEvent.GameId);
            return;
        }

        string message = $"{protectedPlayer.Name} is protected by Handmaid until their next turn.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.HandmaidProtection,
            actingPlayerId: domainEvent.PlayerId, // The player who gained protection
            actingPlayerName: protectedPlayer.Name,
            isPrivate: false,
            message: message,
            playedCardType: CardType.Handmaid // Context: Handmaid play caused this
        );

        game.AddLogEntry(logEntry);
        // As with other similar handlers, PlayCardCommandHandler handles saving the game state
        // before these event handlers are invoked. So, no SaveAsync here.

        await Task.CompletedTask;
    }
}
