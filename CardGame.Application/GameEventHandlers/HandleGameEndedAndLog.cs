using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the GameEnded domain event to log the overall game winner.
/// </summary>
public class HandleGameEndedAndLog : INotificationHandler<DomainEventNotification<GameEnded>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleGameEndedAndLog> _logger;

    public HandleGameEndedAndLog(
        IGameRepository gameRepository,
        ILogger<HandleGameEndedAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<GameEnded> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling GameEnded for Game {GameId}. Winner: {WinnerId}",
            domainEvent.GameId, domainEvent.WinnerPlayerId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for GameEnded event.", domainEvent.GameId);
            return;
        }

        var winner = game.Players.FirstOrDefault(p => p.Id == domainEvent.WinnerPlayerId);
        if (winner == null)
        {
            _logger.LogWarning("Winner Player {WinnerId} not found for GameEnded event in Game {GameId}.",
                domainEvent.WinnerPlayerId, domainEvent.GameId);
            // Log with a generic message if winner details can't be found
            var logMessageGeneric = $"Game ended. Player ({domainEvent.WinnerPlayerId.ToString().Substring(0,8)}) wins the game!";
            var logEntryGeneric = new GameLogEntry(
                eventType: GameLogEventType.GameEnd,
                actingPlayerId: domainEvent.WinnerPlayerId,
                actingPlayerName: "None",
                message: logMessageGeneric,
                isPrivate: false
            );
            game.AddLogEntry(logEntryGeneric);
            _logger.LogInformation("Logged GameEnded (winner details missing): {LogMessage} in Game {GameId}", logMessageGeneric, game.Id);
            return;
        }
        
        var logMessage = $"Game ended. {winner.Name} wins the game!";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.GameEnd,
            actingPlayerId: winner.Id,
            actingPlayerName: winner.Name,
            message: logMessage,
            isPrivate: false // Game end is public
        );

        game.AddLogEntry(logEntry);
        _logger.LogInformation("Logged GameEnded: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
