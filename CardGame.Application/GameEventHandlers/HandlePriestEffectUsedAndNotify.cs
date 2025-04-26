using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped PriestEffectUsed domain event to send the revealed
/// card information (ID and Type) directly to the player who played the Priest.
/// </summary>
public class HandlePriestEffectUsedAndNotify : INotificationHandler<DomainEventNotification<PriestEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;

    // Removed IGameRepository dependency
    // private readonly IGameRepository _gameRepository;
    private readonly ILogger<HandlePriestEffectUsedAndNotify> _logger;

    public HandlePriestEffectUsedAndNotify(
        IPlayerNotifier playerNotifier,
        // Removed IGameRepository dependency
        ILogger<HandlePriestEffectUsedAndNotify> logger)
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        // Removed repository assignment
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DomainEventNotification<PriestEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug(
            "Handling PriestEffectUsed by Player {PriestPlayerId} targeting {TargetPlayerId} in Game {GameId}. Revealed CardId: {RevealedCardId}",
            domainEvent.PriestPlayerId, domainEvent.TargetPlayerId, domainEvent.GameId, domainEvent.RevealedCardId);

        var revealedCardDto = new CardDto
        {
            Id = domainEvent.RevealedCardId, 
            Type = domainEvent.RevealedCardType.Value, 
        };

        // Send the reveal information using the notifier service
        await _playerNotifier.SendPriestRevealAsync(
            requestingPlayerId: domainEvent.PriestPlayerId,
            opponentId: domainEvent.TargetPlayerId,
            revealedCard: revealedCardDto,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}