using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the PrinceEffectFailed domain event to log when a Prince effect fails (e.g., target has no card).
/// </summary>
public class HandlePrinceEffectFailedAndLog : INotificationHandler<DomainEventNotification<PrinceEffectFailed>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandlePrinceEffectFailedAndLog> _logger;

    public HandlePrinceEffectFailedAndLog(
        IGameRepository gameRepository,
        ILogger<HandlePrinceEffectFailedAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PrinceEffectFailed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PrinceEffectFailed: Target {TargetId} in Game {GameId}. Reason: {Reason}",
            domainEvent.TargetPlayerId, domainEvent.GameId, domainEvent.Reason);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PrinceEffectFailed event.", domainEvent.GameId);
            return;
        }

        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetPlayerId);
        if (targetPlayer == null)
        {
             // It's possible the target player might not be in the active list if some other logic removed them
            // For now, we'll log with ID if name isn't found.
            _logger.LogWarning("Target Player {TargetId} not found for PrinceEffectFailed event in Game {GameId}. Logging with ID.",
                domainEvent.TargetPlayerId, domainEvent.GameId);
        }

        string targetPlayerName = targetPlayer?.Name ?? $"Player ({domainEvent.TargetPlayerId.ToString().Substring(0,8)})";
        
        // The domainEvent.Reason should be like "Target hand empty"
        var logMessage = $"Prince effect on {targetPlayerName} failed: {domainEvent.Reason}.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.PrinceEffectFailed,
            // The 'targetPlayer' is the one primarily affected or whose state caused the failure.
            // If we need to know who played the Prince, that would require adding ActorPlayerId to PrinceEffectFailed event.
            // For now, we assume the preceding Prince card play log entry implies the actor.
            actingPlayerId: domainEvent.TargetPlayerId, // Or a system/game actor if no specific 'acting' player
            actingPlayerName: targetPlayerName, 
            message: logMessage,
            isPrivate: false // This failure is generally public knowledge
        );

        game.AddLogEntry(logEntry);
        // await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged PrinceEffectFailed: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
