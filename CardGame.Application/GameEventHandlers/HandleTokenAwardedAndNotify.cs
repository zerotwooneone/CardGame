using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the TokenAwarded event to create a log entry.
/// </summary>
public class HandleTokenAwardedAndNotify : INotificationHandler<DomainEventNotification<TokenAwarded>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleTokenAwardedAndNotify> _logger;

    public HandleTokenAwardedAndNotify(
        IGameRepository gameRepository,
        ILogger<HandleTokenAwardedAndNotify> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<TokenAwarded> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling TokenAwarded event for Game {GameId}, Player {PlayerId}. New count: {NewTokenCount}", 
            domainEvent.GameId, domainEvent.PlayerId, domainEvent.NewTokenCount);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for TokenAwarded event.", domainEvent.GameId);
            return;
        }

        var player = game.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);
        if (player == null)
        {
            _logger.LogWarning("Player {PlayerId} not found in Game {GameId} for TokenAwarded event.", 
                domainEvent.PlayerId, domainEvent.GameId);
            return;
        }

        string message = $"{player.Name} was awarded a token of affection. They now have {domainEvent.NewTokenCount} token(s).";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.TokenAwarded,
            actingPlayerId: domainEvent.PlayerId,
            actingPlayerName: player.Name,
            isPrivate: false,
            message: message
            // No other specific fields needed from comprehensive constructor for this simple event
        );

        game.AddLogEntry(logEntry);
        // The handler that triggered TokenAwarded (e.g., HandleRoundEnded) or a subsequent command handler
        // should be responsible for saving the game state.

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
