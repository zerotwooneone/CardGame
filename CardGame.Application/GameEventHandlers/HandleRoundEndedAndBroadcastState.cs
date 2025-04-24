using CardGame.Application.Common.Notifications;
using CardGame.Application.Common.Services;
using CardGame.Domain.Game.Event;
using MediatR;

namespace CardGame.Application.GameEventHandlers;

/// <summary>
/// Handles the wrapped RoundEnded domain event to trigger a game state broadcast.
/// </summary>
public class HandleRoundEndedAndBroadcastState : INotificationHandler<DomainEventNotification<RoundEnded>>
{
    private readonly GameStateUpdateService _gameStateUpdateService;

    public HandleRoundEndedAndBroadcastState(GameStateUpdateService gameStateUpdateService)
    {
        _gameStateUpdateService = gameStateUpdateService;
    }

    public async Task Handle(DomainEventNotification<RoundEnded> notification, CancellationToken cancellationToken)
    {
        await _gameStateUpdateService.FetchAndBroadcastStateAsync(
            notification.DomainEvent.GameId,
            notification.DomainEvent,
            cancellationToken).ConfigureAwait(false);
    }
}