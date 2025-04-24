using CardGame.Application.Common.Notifications;
using CardGame.Application.Common.Services;
using CardGame.Domain.Game.Event;
using MediatR;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped PlayerPlayedCard domain event to trigger a game state broadcast.
/// </summary>
public class HandlePlayerPlayedCardAndBroadcastState : INotificationHandler<DomainEventNotification<PlayerPlayedCard>>
{
    private readonly GameStateUpdateService _gameStateUpdateService;

    public HandlePlayerPlayedCardAndBroadcastState(GameStateUpdateService gameStateUpdateService)
    {
        _gameStateUpdateService = gameStateUpdateService;
    }

    public async Task Handle(DomainEventNotification<PlayerPlayedCard> notification, CancellationToken cancellationToken)
    {
        // Extract the GameId and call the shared service
        await _gameStateUpdateService.FetchAndBroadcastStateAsync(
            notification.DomainEvent.GameId,
            notification.DomainEvent, // Pass original event for logging context
            cancellationToken).ConfigureAwait(false);
    }
}