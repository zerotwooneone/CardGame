using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles GameEnded events to broadcast the final winner.
/// </summary>
public class HandleGameEndedAndNotify : INotificationHandler<DomainEventNotification<GameEnded>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleGameEndedAndNotify> _logger;

    public HandleGameEndedAndNotify(IPlayerNotifier playerNotifier, ILogger<HandleGameEndedAndNotify> logger)
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<GameEnded> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Handling GameEnded for Game {GameId}. Winner: {WinnerId}", domainEvent.GameId,
            domainEvent.WinnerPlayerId);

        await _playerNotifier.BroadcastGameWinnerAsync(
            domainEvent.GameId,
            domainEvent.WinnerPlayerId,
            cancellationToken).ConfigureAwait(false);
    }
}