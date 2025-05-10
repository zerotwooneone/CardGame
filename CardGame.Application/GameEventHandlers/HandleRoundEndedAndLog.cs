using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the RoundEnded domain event to log the outcome of a round.
/// </summary>
public class HandleRoundEndedAndLog : INotificationHandler<DomainEventNotification<RoundEnded>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleRoundEndedAndLog> _logger;

    public HandleRoundEndedAndLog(
        IGameRepository gameRepository,
        ILogger<HandleRoundEndedAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<RoundEnded> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling RoundEnded for Game {GameId}. Winner: {WinnerId}, Reason: {Reason}",
            domainEvent.GameId, domainEvent.WinnerPlayerId, domainEvent.Reason);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for RoundEnded event.", domainEvent.GameId);
            return;
        }

        string roundEndMessage;
        string winnerName = null;

        if (domainEvent.WinnerPlayerId.HasValue)
        {
            var winnerSummary = domainEvent.PlayerSummaries.FirstOrDefault(ps => ps.PlayerId == domainEvent.WinnerPlayerId.Value);
            winnerName = winnerSummary?.PlayerName ?? $"Player ({domainEvent.WinnerPlayerId.Value.ToString().Substring(0,8)})";
            roundEndMessage = $"Round ended. {winnerName} wins the round! ({domainEvent.Reason}).";
        }
        else
        {
            roundEndMessage = $"Round ended. It's a draw. ({domainEvent.Reason}).";
        }

        // Optionally, append player summaries to the log message or store them in a structured way if needed.
        // For now, the main outcome is logged.

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.RoundEnd,
            // If there's a winner, they are the 'acting' entity in terms of this log event.
            // If no winner, perhaps use a system/game ID or null.
            actingPlayerId: domainEvent.WinnerPlayerId ?? Guid.Empty, 
            actingPlayerName: winnerName, // Null if no winner
            message: roundEndMessage,
            isPrivate: false // Round end is public
        );
        // The GameLogEntry might need adjustment if actingPlayerName cannot be null, 
        // or we use a placeholder like "Game" or an empty string.
        // For now, assuming actingPlayerName can be null if actingPlayerId is null.

        game.AddLogEntry(logEntry);
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged RoundEnded: {RoundEndMessage} in Game {GameId}", roundEndMessage, game.Id);
    }
}
