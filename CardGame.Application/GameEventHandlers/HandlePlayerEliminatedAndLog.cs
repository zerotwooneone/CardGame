using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the PlayerEliminated domain event to log when a player is knocked out of the round.
/// </summary>
public class HandlePlayerEliminatedAndLog : INotificationHandler<DomainEventNotification<PlayerEliminated>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandlePlayerEliminatedAndLog> _logger;

    public HandlePlayerEliminatedAndLog(
        IGameRepository gameRepository,
        ILogger<HandlePlayerEliminatedAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PlayerEliminated> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PlayerEliminated: Player {PlayerId} in Game {GameId}. Reason: {Reason}, CardResponsible: {CardResponsible}",
            domainEvent.PlayerId, domainEvent.GameId, domainEvent.Reason, domainEvent.CardResponsible?.Name);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PlayerEliminated event.", domainEvent.GameId);
            return;
        }

        var eliminatedPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.PlayerId);
        if (eliminatedPlayer == null)
        {
            // Player might have already been removed from the active list in some game states,
            // but we should still be able to log. Let's try to get their name if possible, or use ID.
            // For robust logging, player details (like name) should ideally be part of the event or fetched from a historical record if not in active players.
            _logger.LogWarning("Eliminated Player {PlayerId} not found in active players of Game {GameId}. This might be okay if player was already removed from list.",
                domainEvent.PlayerId, domainEvent.GameId);
            // Attempting to construct a name, or fallback to ID
        }

        string playerName = eliminatedPlayer?.Name ?? $"Player ({domainEvent.PlayerId.ToString().Substring(0,8)})";
        
        // The domainEvent.Reason is quite descriptive, e.g., "guessed correctly by PlayerX with a Guard"
        // or "discarded the Princess".
        // We can choose to use it directly or construct a simpler one.
        // For now, let's use a standard format and include the detailed reason.
        var logMessage = $"{playerName} was knocked out of the round. Reason: {domainEvent.Reason}.";
        if (domainEvent.CardResponsible != null)
        {
            logMessage += $" (Card: {domainEvent.CardResponsible.Name})";
        }

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.PlayerEliminated,
            // For a 'knocked out' event, the 'eliminatedPlayer' is the one primarily affected.
            // If we want to show who 'acted' to knock them out, that info is in the 'Reason' or prior logs.
            actingPlayerId: domainEvent.PlayerId, // The player who got knocked out is the subject.
            actingPlayerName: playerName, 
            message: logMessage,
            isPrivate: false // Player eliminations are public
        );

        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged PlayerEliminated: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
