using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped PlayerPlayedCard domain event to send the updated
/// hand state back to the player who played the card.
/// </summary>
public class HandlePlayerPlayedCardAndSendHandUpdate : INotificationHandler<DomainEventNotification<PlayerPlayedCard>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandlePlayerPlayedCardAndSendHandUpdate> _logger;

    public HandlePlayerPlayedCardAndSendHandUpdate(
        IGameRepository gameRepository,
        IPlayerNotifier playerNotifier,
        ILogger<HandlePlayerPlayedCardAndSendHandUpdate> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PlayerPlayedCard> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerPlayedCard for Player {PlayerId} in Game {GameId} to send hand update.",
            domainEvent.PlayerId, domainEvent.GameId);

        // Load the game state to get the player's current hand AFTER the card was played
        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        var player = game?.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);

        if (player != null)
        {
            // Map the player's current hand (which should now have 1 card) to DTOs
            var handDto = player.Hand.GetCards().Select(c => new CardDto
            {
                AppearanceId = c.AppearanceId, // Ensure DTO has Id
                Rank = c.Rank
            }).ToList();

            // Send the hand update using the notifier service ONLY to the player who acted
            await _playerNotifier.SendHandUpdateAsync(player.Id, handDto, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _logger.LogWarning(
                "Could not find Player {PlayerId} in Game {GameId} to send hand update after PlayerPlayedCard event.",
                domainEvent.PlayerId, domainEvent.GameId);
        }
    }
}