using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq; 

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles RoundEnded events to log the outcome and broadcast the summary.
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
                FinalHeldCard = summary.FinalHeldCard == null
                    ? null
                    : new CardDto
                    {
                        Id = summary.FinalHeldCard.AppearanceId, 
                        Type = summary.FinalHeldCard.Rank 
                    },
                DiscardPileValues = summary.DiscardPileValues, 
                TokensWon = summary.TokensWon
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