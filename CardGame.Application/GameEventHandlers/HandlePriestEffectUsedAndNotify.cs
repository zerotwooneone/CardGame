using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Notifications;
using CardGame.Application.DTOs;
using CardGame.Domain.Game.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using CardGame.Domain.Interfaces;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped PriestEffectUsed domain event to send the revealed
/// card information (ID and Type) directly to the player who played the Priest.
/// </summary>
public class HandlePriestEffectUsedAndNotify : INotificationHandler<DomainEventNotification<PriestEffectUsed>>
{
    private readonly IPlayerNotifier _playerNotifier;
    private readonly ILogger<HandlePriestEffectUsedAndNotify> _logger;
    private readonly IGameRepository _gameRepository; 

    public HandlePriestEffectUsedAndNotify(
        IPlayerNotifier playerNotifier,
        ILogger<HandlePriestEffectUsedAndNotify> logger,
        IGameRepository gameRepository) 
    {
        _playerNotifier = playerNotifier ?? throw new ArgumentNullException(nameof(playerNotifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository)); 
    }

    public async Task Handle(DomainEventNotification<PriestEffectUsed> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogDebug(
            "Handling PriestEffectUsed event for Game {GameId}. Player {PriestPlayerId} targeted {TargetPlayerId}. Revealed Card Type: {RevealedCardType}. Preparing to notify.",
            domainEvent.GameId, domainEvent.PriestPlayerId, domainEvent.TargetPlayerId, domainEvent.RevealedCard.Rank.ToString());

        var game = await _gameRepository.GetByIdAsync(domainEvent.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            _logger.LogWarning("Game {GameId} not found when handling PriestEffectUsed event. Cannot send notification.", domainEvent.GameId);
            return;
        }

        var targetPlayer = game.Players.FirstOrDefault(p => p.Id == domainEvent.TargetPlayerId);
        if (targetPlayer == null)
        {
            _logger.LogWarning("Target Player {TargetPlayerId} not found in Game {GameId} for PriestEffectUsed event. Cannot send notification.", 
                domainEvent.TargetPlayerId, domainEvent.GameId);
            return;
        }
        string targetPlayerName = targetPlayer.Name;

        var revealedCardDto = new CardDto
        {
            AppearanceId = domainEvent.RevealedCardId, 
            RankValue = domainEvent.RevealedCard.Rank   
        };

        try
        {
            await _playerNotifier.SendPriestRevealAsync(
                domainEvent.PriestPlayerId,
                domainEvent.TargetPlayerId,
                targetPlayerName,
                revealedCardDto,
                cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Priest reveal notification successfully sent to player {PriestPlayerId} for game {GameId} regarding target {TargetPlayerId}.", 
                domainEvent.PriestPlayerId, domainEvent.GameId, domainEvent.TargetPlayerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Priest reveal notification to player {PriestPlayerId} for game {GameId}.", 
                domainEvent.PriestPlayerId, domainEvent.GameId);
            // Optionally rethrow or handle further depending on policy
        }
    }
}