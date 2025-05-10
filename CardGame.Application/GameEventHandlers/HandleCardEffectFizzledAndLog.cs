using CardGame.Application.Common.Notifications;
using CardGame.Domain;
using CardGame.Domain.Game; // For CardEffectFizzled when it's moved or if it's directly in Game namespace
// Assuming CardEffectFizzled is here
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the CardEffectFizzled domain event to log when a card's effect fails to apply.
/// </summary>
public class HandleCardEffectFizzledAndLog : INotificationHandler<DomainEventNotification<CardEffectFizzled>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleCardEffectFizzledAndLog> _logger;

    public HandleCardEffectFizzledAndLog(
        IGameRepository gameRepository,
        ILogger<HandleCardEffectFizzledAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<CardEffectFizzled> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling CardEffectFizzled: Actor {ActorId} targeted {TargetId} with {CardType} in Game {GameId}. Reason: {Reason}",
            domainEvent.ActorId, domainEvent.TargetId, domainEvent.CardType.Name, domainEvent.GameId, domainEvent.Reason);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for CardEffectFizzled event.", domainEvent.GameId);
            return;
        }

        var actingPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.ActorId);
        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetId);

        if (actingPlayer == null || targetPlayer == null)
        {
            _logger.LogWarning("Actor or Target Player not found for CardEffectFizzled event in Game {GameId}. ActorId: {ActorId}, TargetId: {TargetId}",
                domainEvent.GameId, domainEvent.ActorId, domainEvent.TargetId);
            return;
        }

        // Example: "Alice's Guard effect on Bob fizzled: Target was protected by Handmaid."
        var logMessage = $"{actingPlayer.Name}'s {domainEvent.CardType.Name} effect on {targetPlayer.Name} fizzled: {domainEvent.Reason}.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.EffectFizzled,
            actingPlayerId: actingPlayer.Id,
            actingPlayerName: actingPlayer.Name,
            // We can store target player info as well if the visual log needs it, even if it's in the message.
            // TargetPlayerId: targetPlayer.Id, 
            // TargetPlayerName: targetPlayer.Name,
            message: logMessage,
            isPrivate: false // Effect fizzling is public knowledge
        );

        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged CardEffectFizzled: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
