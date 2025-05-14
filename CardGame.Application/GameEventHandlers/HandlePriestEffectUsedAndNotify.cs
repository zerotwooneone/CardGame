using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain; 
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped PriestEffectUsed domain event to send the revealed
/// card information (ID and Type) directly to the player who played the Priest.
/// </summary>
public class HandlePriestEffectUsedAndNotify : INotificationHandler<DomainEventNotification<PriestEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandlePriestEffectUsedAndNotify> _logger;

    public HandlePriestEffectUsedAndNotify(
        IPlayerNotifier playerNotifier,
        ILogger<HandlePriestEffectUsedAndNotify> logger)
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(DomainEventNotification<PriestEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug(
            "Handling PriestEffectUsed event for Game {GameId}. Player {PriestPlayerId} targeted {TargetPlayerId}. Revealed Card Type: {RevealedCardType}. Private log created in Game.cs.",
            domainEvent.GameId, domainEvent.PriestPlayerId, domainEvent.TargetPlayerId, domainEvent.RevealedCardType.ToString());


        _logger.LogInformation("Priest reveal notification sent to player {PriestPlayerId} for game {GameId}.", domainEvent.PriestPlayerId, domainEvent.GameId);

        return Task.CompletedTask;
    }
}