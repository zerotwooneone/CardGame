using CardGame.Application.Common.Notifications;
using CardGame.Application.Common.Services;
using CardGame.Domain.Game.Event;
using MediatR;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped GameCreated domain event to trigger a game state broadcast.
/// </summary>
public class HandleGameCreatedAndBroadcastState : INotificationHandler<DomainEventNotification<GameCreated>>
{
    private readonly GameStateUpdateService _gameStateUpdateService;

    public HandleGameCreatedAndBroadcastState(GameStateUpdateService gameStateUpdateService)
    {
        _gameStateUpdateService = gameStateUpdateService;
    }

    public async Task Handle(DomainEventNotification<GameCreated> notification, CancellationToken cancellationToken)
    {
        // Extract the GameId and call the shared service
        await _gameStateUpdateService.FetchAndBroadcastStateAsync(
            notification.DomainEvent.GameId,
            notification.DomainEvent, // Pass original event for logging context
            cancellationToken).ConfigureAwait(false);
    }
}