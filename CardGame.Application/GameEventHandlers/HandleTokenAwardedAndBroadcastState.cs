using CardGame.Application.Common.Notifications;
using CardGame.Application.Common.Services;
using CardGame.Domain.Game.Event;
using MediatR;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped TokenAwarded domain event to trigger a game state broadcast.
/// </summary>
public class HandleTokenAwardedAndBroadcastState : INotificationHandler<DomainEventNotification<TokenAwarded>>
{
    private readonly GameStateUpdateService _gameStateUpdateService;

    public HandleTokenAwardedAndBroadcastState(GameStateUpdateService gameStateUpdateService)
    {
        _gameStateUpdateService = gameStateUpdateService;
    }

    public async Task Handle(DomainEventNotification<TokenAwarded> notification, CancellationToken cancellationToken)
    {
        await _gameStateUpdateService.FetchAndBroadcastStateAsync(
            notification.DomainEvent.GameId,
            notification.DomainEvent,
            cancellationToken).ConfigureAwait(false);
    }
}