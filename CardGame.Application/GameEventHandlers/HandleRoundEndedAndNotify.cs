using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
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

        // --- Construct the RoundEndSummaryDto ---
        var playerSummaryDtos = domainEvent.PlayerSummaries.Select(summary =>
            new RoundEndPlayerSummaryDto // Map domain summary to DTO summary
            {
                PlayerId = summary.PlayerId, // Convert Guid
                PlayerName = summary.PlayerName,
                // Map final held Card to CardDto (handle null)
                FinalHeldCard = summary.FinalHeldCard == null
                    ? null
                    : new CardDto
                    {
                        Id = summary.FinalHeldCard.Id, // Convert Guid
                        Type = summary.FinalHeldCard.Rank // Use Rank (int)
                    },
                DiscardPileValues = summary.DiscardPileValues, // Already List<int>
                TokensWon = summary.TokensWon
            }).ToList();

        var summaryData = new RoundEndSummaryDto
        {
            Reason = domainEvent.Reason,
            WinnerPlayerId = domainEvent.WinnerPlayerId, // Convert Guid?
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