using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles BaronComparisonResult events to broadcast the outcome.
/// </summary>
public class HandleBaronComparisonResultAndNotify : INotificationHandler<DomainEventNotification<BaronComparisonResult>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleBaronComparisonResultAndNotify> _logger;
    private readonly IGameRepository _gameRepository; 

    public HandleBaronComparisonResultAndNotify(IPlayerNotifier playerNotifier,
        ILogger<HandleBaronComparisonResultAndNotify> logger,
        IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
        _gameRepository = gameRepository; 
    }

    public async Task Handle(DomainEventNotification<BaronComparisonResult> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling BaronComparisonResult for Game {GameId}. Player1: {Player1Id} (Card: {Player1Card}), Player2: {Player2Id} (Card: {Player2Card}), Loser: {LoserPlayerId}", 
            domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player1Card, domainEvent.Player2Id, domainEvent.Player2Card, domainEvent.LoserPlayerId);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for BaronComparisonResult event.", domainEvent.GameId);
            return;
        }

        var player1 = game.Players.FirstOrDefault(p => p.Id == domainEvent.Player1Id);
        var player2 = game.Players.FirstOrDefault(p => p.Id == domainEvent.Player2Id);

        if (player1 == null || player2 == null)
        {
            _logger.LogWarning("Player1 or Player2 not found for BaronComparisonResult event in Game {GameId}. Player1Id: {Player1Id}, Player2Id: {Player2Id}", 
                domainEvent.GameId, domainEvent.Player1Id, domainEvent.Player2Id);
            return;
        }

        // Format the message for the log entry
        // Assuming domainEvent.Player1Card and .Player2Card are CardGame.Domain.Types.CardType
        string player1CardName = domainEvent.Player1Card.ToString(); // Or a more friendly name
        string player2CardName = domainEvent.Player2Card.ToString(); // Or a more friendly name
        string message;
        if (domainEvent.LoserPlayerId.HasValue)
        {
            string loserName = domainEvent.LoserPlayerId == player1.Id ? player1.Name : player2.Name;
            message = $"{player1.Name} (holding {player1CardName}) used Baron against {player2.Name} (holding {player2CardName}). {loserName} lost the comparison and was knocked out.";
        }
        else
        {
            message = $"{player1.Name} (holding {player1CardName}) used Baron against {player2.Name} (holding {player2CardName}). It was a tie, no one was knocked out.";
        }

        var logEntry = new GameLogEntry(
            eventType: GameLogEventType.BaronCompare,
            actingPlayerId: domainEvent.Player1Id, 
            actingPlayerName: player1.Name,
            isPrivate: false, // Baron comparison results are public
            message: message, // Keep pre-formatted message for now
            targetPlayerId: domainEvent.Player2Id,
            targetPlayerName: player2.Name,
            playedCardType: CardType.Baron, // Explicitly set the card played
            player1ComparedCardType: domainEvent.Player1Card, // Player 1's card in comparison
            player2ComparedCardType: domainEvent.Player2Card, // Player 2's card in comparison
            baronLoserPlayerId: domainEvent.LoserPlayerId // ID of the player who lost, if any
        );
        game.AddLogEntry(logEntry);

        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        // Deprecated SignalR broadcast, game state updates will carry log
        // await _playerNotifier.BroadcastBaronComparisonAsync(
        //     domainEvent.GameId,
        //     domainEvent.Player1Id,
        //     domainEvent.Player1Card.Value, 
        //     domainEvent.Player2Id,
        //     domainEvent.Player2Card.Value, 
        //     domainEvent.LoserPlayerId,
        //     cancellationToken).ConfigureAwait(false);
    }
}