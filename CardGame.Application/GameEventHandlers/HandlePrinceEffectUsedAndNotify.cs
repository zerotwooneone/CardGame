using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain; 
using CardGame.Domain.Interfaces; 
using System.Linq;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles PrinceEffectUsed events to log the discard outcome.
/// (Note: PlayerDrewCard handler handles sending the new hand state privately).
/// </summary>
public class HandlePrinceEffectUsedAndNotify : INotificationHandler<DomainEventNotification<PrinceEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandlePrinceEffectUsedAndNotify> _logger;
    private readonly IGameRepository _gameRepository; 

    public HandlePrinceEffectUsedAndNotify(IPlayerNotifier playerNotifier,
        ILogger<HandlePrinceEffectUsedAndNotify> logger,
        IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier;
        _logger = logger;
        _gameRepository = gameRepository; 
    }

    public async Task Handle(DomainEventNotification<PrinceEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling PrinceEffectUsed by {ActorPlayerId} on {TargetPlayerId} in Game {GameId}. Discarded: {DiscardedCardType}", 
            domainEvent.ActorPlayerId, domainEvent.TargetPlayerId, domainEvent.GameId, domainEvent.DiscardedCardType);

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found for PrinceEffectUsed event.", domainEvent.GameId);
            return;
        }

        // Log Entry 1 (Prince Played) is now handled by HandlePlayerPlayedCardAndNotify.

        // Log Entry 2: Card Discarded due to Prince
        var actorPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.ActorPlayerId);
        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetPlayerId);

        if (actorPlayer == null || targetPlayer == null) // Should have been caught earlier, but good check
        {
            _logger.LogWarning("Actor or Target Player not found for PrinceEffectUsed event in Game {GameId}. ActorId: {ActorId}, TargetId: {TargetId}", 
                domainEvent.GameId, domainEvent.ActorPlayerId, domainEvent.TargetPlayerId);
            return;
        }

        string discardedCardName = domainEvent.DiscardedCardType.ToString();
        string discardMessage = $"{targetPlayer.Name} was forced by Prince to discard a {discardedCardName}.";
        if (domainEvent.DiscardedCardType == CardType.Princess)
        {
            discardMessage += $" It was the Princess! {targetPlayer.Name} is knocked out of the round.";
        }

        var princeDiscardLogEntry = new GameLogEntry(
            eventType: GameLogEventType.PrinceDiscard,
            actingPlayerId: domainEvent.TargetPlayerId, // Player who was forced to discard
            actingPlayerName: targetPlayer.Name,
            isPrivate: false,
            message: discardMessage,
            targetPlayerId: domainEvent.ActorPlayerId, // Linking back to who played the Prince
            targetPlayerName: actorPlayer.Name, // Name of the player who played prince
            playedCardType: CardType.Prince, // Card that caused the discard (Prince)
            discardedByPrinceCardType: domainEvent.DiscardedCardType
        );
        game.AddLogEntry(princeDiscardLogEntry);

        // No SaveAsync here, PlayCardCommandHandler handles saving before event publishing.
        await Task.CompletedTask;
    }
}