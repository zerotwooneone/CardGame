using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles KingEffectUsed events to send updated hand states to both involved players.
/// </summary>
public class HandleKingEffectUsedAndNotify : INotificationHandler<DomainEventNotification<KingEffectUsed>>
{
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandleKingEffectUsedAndNotify> _logger;

    public HandleKingEffectUsedAndNotify(
        IGameRepository gameRepository,
        IPlayerNotifier playerNotifier,
        ILogger<HandleKingEffectUsedAndNotify> logger)
    {
        _gameRepository = gameRepository;
        _playerNotifier = playerNotifier;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<KingEffectUsed> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug("Handling KingEffectUsed between Player {Player1Id} and Player {Player2Id} in Game {GameId}",
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
            _logger.LogWarning("One or both players not found for KingEffectUsed. P1: {Player1Id}, P2: {Player2Id}, Game: {GameId}", 
                domainEvent.Player1Id, domainEvent.Player2Id, domainEvent.GameId);
            return;
        }

        // The GameLogEntry for playing the King is now handled by HandlePlayerPlayedCardAndNotify.
        // This handler's responsibility is to notify players of their new hands, which it already does.
        // No specific "effect" log beyond the card play itself is typically needed for King,
        // as the hand swap is reflected in player states.

        // Ensure game state is saved if this handler was previously responsible for it due to logging.
        // However, PlayCardCommandHandler saves *before* events are published. PlayerNotifier might save if it modifies state.
        // For now, assuming no save is needed here as it primarily notifies.
        // If playerNotifier.SendHandUpdateAsync could indirectly modify and require a save, that's a separate concern.
        // Based on current refactoring, log-related SaveAsync calls are removed from these event-specific handlers.

        // Send update to Player 1
        if (player1 != null)
        {
            var hand1Dto = player1.Hand.GetCards()
                .Select(c => new CardDto {Rank = c.Rank, AppearanceId = c.AppearanceId}).ToList();
            await _playerNotifier.SendHandUpdateAsync(player1.Id, hand1Dto, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _logger.LogWarning("Player {PlayerId} not found for KingEffectUsed event in Game {GameId}.",
                domainEvent.Player1Id, domainEvent.GameId);
        }

        // Send update to Player 2
        if (player2 != null)
        {
            var hand2Dto = player2.Hand.GetCards()
                .Select(c => new CardDto {Rank = c.Rank, AppearanceId = c.AppearanceId}).ToList();
            await _playerNotifier.SendHandUpdateAsync(player2.Id, hand2Dto, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _logger.LogWarning("Player {PlayerId} not found for KingEffectUsed event in Game {GameId}.",
                domainEvent.Player2Id, domainEvent.GameId);
        }
    }
}