using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the PlayerEliminated event to create a log entry detailing the elimination.
/// </summary>
public class HandlePlayerEliminatedAndNotify : INotificationHandler<DomainEventNotification<PlayerEliminated>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandlePlayerEliminatedAndNotify> _logger;

    public HandlePlayerEliminatedAndNotify(
        IGameRepository gameRepository,
        ILogger<HandlePlayerEliminatedAndNotify> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PlayerEliminated> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerEliminated event for Game {GameId}, Player {PlayerId}. Reason: {Reason}", 
            domainEvent.GameId, domainEvent.PlayerId, domainEvent.Reason);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PlayerEliminated event.", domainEvent.GameId);
            return;
        }

        var eliminatedPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);
        if (eliminatedPlayer == null) // Should ideally not happen if event is valid
        {
            _logger.LogWarning("Eliminated player {PlayerId} not found in Game {GameId} for PlayerEliminated event.", 
                domainEvent.PlayerId, domainEvent.GameId);
            return;
        }

        string message = $"{eliminatedPlayer.Name} was eliminated because they {domainEvent.Reason}.";
        if (domainEvent.CardResponsible == CardType.Princess && domainEvent.Reason.Contains("discarded the Princess"))
        {
             // A more specific message for Princess discard leading to elimination
            message = $"{eliminatedPlayer.Name} discarded the Princess and is knocked out of the round!";
        }
        else if (domainEvent.CardResponsible != null)
        {
            message += $" (Card involved: {domainEvent.CardResponsible.Name})";
        }

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.PlayerEliminated,
            actingPlayerId: domainEvent.PlayerId, // The player who was eliminated
            actingPlayerName: eliminatedPlayer.Name,
            isPrivate: false,
            message: message,
            // If the elimination was due to a card play (e.g., Baron loss, Princess discard),
            // this field links to that card type.
            playedCardType: domainEvent.CardResponsible 
            // wasEliminated: true // Removed - EventType implies this
        );

        game.AddLogEntry(logEntry);
        // PlayCardCommandHandler or other command handlers (like one that might process round end) would save.
        // This handler solely adds the log entry based on the event.

        await Task.CompletedTask;
    }
}
