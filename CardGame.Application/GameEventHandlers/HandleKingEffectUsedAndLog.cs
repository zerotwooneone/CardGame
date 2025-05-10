using CardGame.Domain;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the KingEffectUsed domain event to log when players trade hands.
/// </summary>
public class HandleKingEffectUsedAndLog : INotificationHandler<DomainEventNotification<KingEffectUsed>>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandleKingEffectUsedAndLog> _logger;

    public HandleKingEffectUsedAndLog(
        IGameRepository gameRepository,
        ILogger<HandleKingEffectUsedAndLog> logger)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<KingEffectUsed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling KingEffectUsed: Player1 {Player1Id} traded with Player2 {Player2Id} in Game {GameId}",
            domainEvent.Player1Id, domainEvent.Player2Id, domainEvent.GameId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for KingEffectUsed event.", domainEvent.GameId);
            return;
        }

        var player1 = game.Players.FirstOrDefault(p => p.Id == domainEvent.Player1Id);
        var player2 = game.Players.FirstOrDefault(p => p.Id == domainEvent.Player2Id);

        if (player1 == null || player2 == null)
        {
            _logger.LogWarning("Player1 or Player2 not found for KingEffectUsed event in Game {GameId}. P1Id: {P1Id}, P2Id: {P2Id}",
                domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player2Id);
            return;
        }
        
        var logMessage = $"{player1.Name} used King to trade hands with {player2.Name}.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.KingTrade,
            actingPlayerId: player1.Id,
            actingPlayerName: player1.Name,
            targetPlayerId: player2.Id,
            targetPlayerName: player2.Name,
            message: logMessage,
            isPrivate: false // King trades are public knowledge
        );

        game.AddLogEntry(logEntry);
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logged KingEffectUsed: {LogMessage} in Game {GameId}", logMessage, game.Id);
    }
}
