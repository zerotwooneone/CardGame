using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles PlayerDrewCard events to send an updated hand state to the player.
/// </summary>
public class HandlePlayerDrewCardAndNotify : INotificationHandler<DomainEventNotification<PlayerDrewCard>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandlePlayerDrewCardAndNotify> _logger;

    public HandlePlayerDrewCardAndNotify(
        IGameRepository gameRepository,
        IPlayerNotifier playerNotifier,
        ILogger<HandlePlayerDrewCardAndNotify> logger)
    {
        _gameRepository = gameRepository;
        _playerNotifier = playerNotifier;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<PlayerDrewCard> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerDrewCard for Player {PlayerId} in Game {GameId}", domainEvent.PlayerId,
            domainEvent.GameId);

        // Need to load the game state to get the player's current hand
        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        var player = game?.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);

        if (player != null)
        {
            // Map the player's current hand to DTOs
            var handDto = player.Hand.GetCards().Select(c => new CardDto
            {
                AppearanceId = c.AppearanceId, 
                Rank = c.Rank.Value,
            }).ToList();

            // Send the hand update using the notifier service
            await _playerNotifier.SendHandUpdateAsync(player.Id, handDto, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _logger.LogWarning(
                "Could not find Player {PlayerId} in Game {GameId} to send hand update after PlayerDrewCard event.",
                domainEvent.PlayerId, domainEvent.GameId);
        }
    }
}