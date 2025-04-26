using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles GuardGuessResult events to broadcast the outcome.
/// </summary>
public class HandleGuardGuessResultAndNotify : INotificationHandler<DomainEventNotification<GuardGuessResult>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleGuardGuessResultAndNotify> _logger;

    public HandleGuardGuessResultAndNotify(IPlayerNotifier playerNotifier,
        ILogger<HandleGuardGuessResultAndNotify> logger)
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<GuardGuessResult> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling GuardGuessResult for Game {GameId}.", domainEvent.GameId);

        await _playerNotifier.BroadcastGuardGuessAsync(
            domainEvent.GameId,
            domainEvent.GuesserId,
            domainEvent.TargetId,
            domainEvent.GuessedCardType.Value, // Send name string
            domainEvent.WasCorrect,
            cancellationToken).ConfigureAwait(false);
    }
}