using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles RoundEnded events to broadcast the winner and reason.
/// </summary>
public class HandleRoundEndedAndNotify : INotificationHandler<DomainEventNotification<RoundEnded>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleRoundEndedAndNotify> _logger;

    public HandleRoundEndedAndNotify(IPlayerNotifier playerNotifier, ILogger<HandleRoundEndedAndNotify> logger)
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<RoundEnded> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Handling RoundEnded for Game {GameId}. Winner: {WinnerId}", domainEvent.GameId,
            domainEvent.WinnerPlayerId ?? Guid.Empty);

        // Convert final hands CardType? to int? for the notification payload
        // Ensure null CardType maps to null int
        var finalHandsPayload = domainEvent.FinalHands
            .ToDictionary(
                kvp => kvp.Key, // PlayerId
                kvp => kvp.Value?.Value // CardType integer value, or null if CardType is null
            );

        await _playerNotifier.BroadcastRoundWinnerAsync(
            domainEvent.GameId,
            domainEvent.WinnerPlayerId,
            domainEvent.Reason,
            finalHandsPayload, // Send dictionary with int? values
            cancellationToken).ConfigureAwait(false);
    }
}