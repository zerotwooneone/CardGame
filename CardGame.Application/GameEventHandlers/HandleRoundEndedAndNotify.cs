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
    private readonly IGameRepository _gameRepository; 

    public HandleRoundEndedAndNotify(IPlayerNotifier playerNotifier, 
                                   ILogger<HandleRoundEndedAndNotify> logger,
                                   IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository)); 
    }

    public async Task Handle(DomainEventNotification<RoundEnded> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Handling RoundEnded for Game {GameId}. Winner: {WinnerId}, Reason: {Reason}", 
            domainEvent.GameId, domainEvent.WinnerPlayerId ?? Guid.Empty, domainEvent.Reason);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for RoundEnded event.", domainEvent.GameId);
            return;
        }

        string winnerName = null;
        if (domainEvent.WinnerPlayerId.HasValue)
        {
            var winnerPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.WinnerPlayerId.Value);
            if (winnerPlayer != null)
            {
                winnerName = winnerPlayer.Name;
            }
            else
            {
                _logger.LogWarning("Winner player with ID {WinnerId} not found in game {GameId} for RoundEnded event.", domainEvent.WinnerPlayerId.Value, domainEvent.GameId);
            }
        }

        // Format the message for the log entry
        string message;
        if (winnerName != null)
        {
            message = $"Round ended. {winnerName} wins! Reason: {domainEvent.Reason}.";
        }
        else
        {
            message = $"Round ended. Reason: {domainEvent.Reason}. No winner declared (e.g., all players out or deck empty).";
        }

        // Add details about final hands if desired, for example:
        // var finalHandsSummary = string.Join(", ", domainEvent.PlayerSummaries.Select(ps => $"{ps.PlayerName} ({ps.FinalHeldCard?.Rank ?? 'No Card'}) [{ps.TokensWon} tokens]"));
        // message += $" Final hands: {finalHandsSummary}.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.RoundEnd, 
            message: message 
        );
        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        // --- Construct the RoundEndSummaryDto ---
        var playerSummaryDtos = domainEvent.PlayerSummaries.Select(summary =>
            new RoundEndPlayerSummaryDto 
            {
                PlayerId = summary.PlayerId, 
                PlayerName = summary.PlayerName,
                FinalHeldCard = summary.FinalHeldCard == null
                    ? null
                    : new CardDto
                    {
                        Id = summary.FinalHeldCard.Id, 
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