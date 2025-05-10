using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles GuardGuessResult events to broadcast the outcome.
/// </summary>
public class HandleGuardGuessResultAndNotify : INotificationHandler<DomainEventNotification<GuardGuessResult>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleGuardGuessResultAndNotify> _logger;
    private readonly IGameRepository _gameRepository; 

    public HandleGuardGuessResultAndNotify(IPlayerNotifier playerNotifier,
        ILogger<HandleGuardGuessResultAndNotify> logger,
        IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
        _gameRepository = gameRepository; 
    }

    public async Task Handle(DomainEventNotification<GuardGuessResult> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling GuardGuessResult for Game {GameId}. Guesser: {GuesserId}, Target: {TargetId}, Guessed: {GuessedCardType}, Correct: {WasCorrect}", 
            domainEvent.GameId, domainEvent.GuesserId, domainEvent.TargetId, domainEvent.GuessedCardType, domainEvent.WasCorrect);

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
            _logger.LogWarning("Guesser or Target Player not found for GuardGuessResult event in Game {GameId}. GuesserId: {GuesserId}, TargetPlayerId: {TargetId}", 
                domainEvent.GameId, domainEvent.GuesserId, domainEvent.TargetId);
            return;
        }

        // Format the message for the log entry
        // Assuming domainEvent.GuessedCardType is of CardGame.Domain.Types.CardType enum
        string guessedCardName = domainEvent.GuessedCardType.ToString(); // Or a more friendly name if available
        string outcome = domainEvent.WasCorrect ? "correct" : "incorrect";
        string message = $"{guesserPlayer.Name} used Guard, guessing that {targetPlayer.Name} had a {guessedCardName}. The guess was {outcome}.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.GuardGuess,
            actingPlayerId: domainEvent.GuesserId,
            actingPlayerName: guesserPlayer.Name,
            isPrivate: false, // Guard guess results are public
            message: message, // Keep pre-formatted message for now
            targetPlayerId: domainEvent.TargetId,
            targetPlayerName: targetPlayer.Name,
            playedCardType: CardType.Guard, // Explicitly set the card played
            guessedCardType: domainEvent.GuessedCardType, // The card that was guessed
            wasGuessCorrect: domainEvent.WasCorrect
        );
        game.AddLogEntry(logEntry);

        // await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        // Deprecated SignalR broadcast, game state updates will carry log
        // await _playerNotifier.BroadcastGuardGuessAsync(
        //     domainEvent.GameId,
        //     domainEvent.GuesserId,
        //     domainEvent.TargetId,
        //     domainEvent.GuessedCardType.Value, // Send name string
        //     domainEvent.WasCorrect,
        //     cancellationToken).ConfigureAwait(false);
    }
}