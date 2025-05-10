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
    private readonly ILogger<HandleGuardGuessResultAndNotify> _logger;

    public HandleGuardGuessResultAndNotify(ILogger<HandleGuardGuessResultAndNotify> logger) 
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<GuardGuessResult> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling GuardGuessResult for Game {GameId}. Guesser: {GuesserId}, Target: {TargetId}, Guessed: {GuessedCardType}, Correct: {WasCorrect}", 
            domainEvent.GameId, domainEvent.GuesserId, domainEvent.TargetId, domainEvent.GuessedCardType, domainEvent.WasCorrect);

        return Task.CompletedTask;
    }
}