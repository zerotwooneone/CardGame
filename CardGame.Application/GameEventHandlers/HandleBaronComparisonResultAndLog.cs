using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the BaronComparisonResult domain event to log the outcome of a Baron card play.
/// </summary>
public class HandleBaronComparisonResultAndLog : INotificationHandler<DomainEventNotification<BaronComparisonResult>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleBaronComparisonResultAndLog> _logger;

    public HandleBaronComparisonResultAndLog(
        IGameRepository gameRepository,
        ILogger<HandleBaronComparisonResultAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<BaronComparisonResult> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling BaronComparisonResult: Player1 {Player1Id} ({Player1Card}) vs Player2 {Player2Id} ({Player2Card}) in Game {GameId}. Loser: {LoserPlayerId}",
            domainEvent.Player1Id, domainEvent.Player1Card.Name, domainEvent.Player2Id, domainEvent.Player2Card.Name, domainEvent.GameId, domainEvent.LoserPlayerId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for BaronComparisonResult event.", domainEvent.GameId);
            return;
        }

        var player1 = game.Players.FirstOrDefault(p => p.Id == domainEvent.Player1Id);
        var player2 = game.Players.FirstOrDefault(p => p.Id == domainEvent.Player2Id);

        if (player1 == null || player2 == null)
        {
            _logger.LogWarning("Player1 or Player2 not found for BaronComparisonResult event in Game {GameId}. P1Id: {P1Id}, P2Id: {P2Id}",
                domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player2Id);
            return;
        }

        string outcomeDetail;
        if (domainEvent.LoserPlayerId == null)
        {
            outcomeDetail = "It was a draw.";
        }
        else if (domainEvent.LoserPlayerId == domainEvent.Player1Id)
        {
            outcomeDetail = $"{player2.Name} won. {player1.Name} is knocked out.";
        }
        else
        {
            outcomeDetail = $"{player1.Name} won. {player2.Name} is knocked out.";
        }
        
        var logMessage = $"{player1.Name} (with {domainEvent.Player1Card.Name}) used Baron to compare with {player2.Name} (with {domainEvent.Player2Card.Name}). {outcomeDetail}";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.BaronCompare,
            actingPlayerId: player1.Id, // Player1 is the one who initiated the Baron play
            actingPlayerName: player1.Name,
            targetPlayerId: player2.Id,
            targetPlayerName: player2.Name,
            // We can include revealed cards if needed for structured display, though they are in the message.
            // RevealedCardId for player1, RevealedCardType for player1
            // AnotherRevealedCardId for player2, AnotherRevealedCardType for player2 (if GameLogEntry supports multiple)
            message: logMessage,
            isPrivate: false // Baron comparisons are public
        );

        game.AddLogEntry(logEntry);
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged BaronComparisonResult: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
