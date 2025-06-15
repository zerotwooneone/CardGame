using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles RoundEnded domain events. The primary GameLogEntry for the round's outcome is created in Game.EndRound.
/// This handler uses ILogger for application-level information and broadcasts the round summary to players.
/// </summary>
public class HandleRoundEndedAndNotify : INotificationHandler<DomainEventNotification<RoundEnded>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleRoundEndedAndNotify> _logger;

    public HandleRoundEndedAndNotify(IPlayerNotifier playerNotifier, 
                                   ILogger<HandleRoundEndedAndNotify> logger) 
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<RoundEnded> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Handling RoundEnded for Game {GameId}. Winner: {WinnerId}, Reason: {Reason}", 
            domainEvent.GameId, domainEvent.WinnerPlayerId ?? Guid.Empty, domainEvent.Reason);
        

        // --- Construct the RoundEndSummaryDto for broadcasting --- 
        var playerSummaryDtos = domainEvent.PlayerSummaries.Select(summary =>
            new RoundEndPlayerSummaryDto 
            {
                PlayerId = summary.PlayerId, 
                PlayerName = summary.PlayerName,
                CardsHeld = summary.CardsHeld.Select(card => new CardDto
                {
                    AppearanceId = card.AppearanceId,
                    RankValue = card.Rank
                }).ToList(),
                TokensWon = summary.TokensAwarded
            }).ToList();

        var summaryData = new RoundEndSummaryDto
        {
            Reason = domainEvent.Reason,
            WinnerPlayerId = domainEvent.WinnerPlayerId, 
            PlayerSummaries = playerSummaryDtos
        };
        // --- End DTO Construction ---

        // Call the updated notifier method with the new DTO
        await _playerNotifier.BroadcastRoundSummaryAsync(
            domainEvent.GameId,
            summaryData,
            cancellationToken).ConfigureAwait(false);
    }
}