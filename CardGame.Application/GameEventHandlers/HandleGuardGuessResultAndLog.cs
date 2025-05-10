using CardGame.Application.Common.Notifications;
using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the GuardGuessResult domain event to log the outcome of a Guard's guess.
/// </summary>
public class HandleGuardGuessResultAndLog : INotificationHandler<DomainEventNotification<GuardGuessResult>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleGuardGuessResultAndLog> _logger;

    public HandleGuardGuessResultAndLog(
        IGameRepository gameRepository,
        ILogger<HandleGuardGuessResultAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<GuardGuessResult> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling GuardGuessResult: Guesser {GuesserId} targeted {TargetId} with guess {GuessedCardType} in Game {GameId}. Correct: {WasCorrect}",
            domainEvent.GuesserId, domainEvent.TargetId, domainEvent.GuessedCardType.Name, domainEvent.GameId, domainEvent.WasCorrect);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for GuardGuessResult event.", domainEvent.GameId);
            return;
        }

        var guesserPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.GuesserId);
        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetId);

        if (guesserPlayer == null || targetPlayer == null)
        {
            _logger.LogWarning("Guesser or Target Player not found for GuardGuessResult event in Game {GameId}. GuesserId: {GuesserId}, TargetId: {TargetId}",
                domainEvent.GameId, domainEvent.GuesserId, domainEvent.TargetId);
            return;
        }

        string outcomeMessage = domainEvent.WasCorrect 
            ? $"Correct! {targetPlayer.Name} is knocked out."
            : "Incorrect.";
        
        var logMessage = $"{guesserPlayer.Name} used Guard on {targetPlayer.Name}, guessing {domainEvent.GuessedCardType.Name}. {outcomeMessage}";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.GuardGuess,
            actingPlayerId: guesserPlayer.Id,
            actingPlayerName: guesserPlayer.Name,
            // Storing target info directly in the log entry can be useful for structured display later
            // even if it's also in the message.
            targetPlayerId: targetPlayer.Id, 
            targetPlayerName: targetPlayer.Name,
            message: logMessage, // The message combines all info for now
            isPrivate: false // Guard guesses and outcomes are public
        );
        // We could also add domainEvent.GuessedCardType to the logEntry if needed for specific UI rendering
        // logEntry.RevealedCardType = domainEvent.GuessedCardType; // Misusing 'Revealed' for 'Guessed'

        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged GuardGuessResult: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
