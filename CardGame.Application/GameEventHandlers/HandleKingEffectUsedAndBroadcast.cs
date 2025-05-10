using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq; 

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles KingEffectUsed events to log the swap outcome.
/// (Note: HandleKingEffectUsedAndNotify in hand_update_handlers handles sending private hand updates).
/// </summary>
public class HandleKingEffectUsedAndBroadcast : INotificationHandler<DomainEventNotification<KingEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleKingEffectUsedAndBroadcast> _logger;
    private readonly IGameRepository _gameRepository; 

    public HandleKingEffectUsedAndBroadcast(IPlayerNotifier playerNotifier,
        ILogger<HandleKingEffectUsedAndBroadcast> logger,
        IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
        _gameRepository = gameRepository; 
    }

    public async Task Handle(DomainEventNotification<KingEffectUsed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling KingEffectUsed for Game {GameId} between {Player1Id} (Actor) and {Player2Id} (Target).",
            domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player2Id);

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
            _logger.LogWarning("Player1 or Player2 not found for KingEffectUsed event in Game {GameId}. Player1Id: {Player1Id}, Player2Id: {Player2Id}", 
                domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player2Id);
            return;
        }

        string message = $"{player1.Name} used King and swapped hands with {player2.Name}.";

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.KingTrade, 
            actingPlayerId: domainEvent.Player1Id, 
            actingPlayerName: player1.Name,
            targetPlayerId: domainEvent.Player2Id,   
            targetPlayerName: player2.Name,
            message: message, 
            isPrivate: false 
        );
        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        // await _playerNotifier.BroadcastKingSwapAsync(
        //     domainEvent.GameId,
        //     domainEvent.Player1Id,
        //     domainEvent.Player2Id,
        //     cancellationToken).ConfigureAwait(false);
    }
}