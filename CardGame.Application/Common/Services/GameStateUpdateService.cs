using CardGame.Application.Common.Interfaces;
using CardGame.Application.Queries;
using CardGame.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.Common.Services;

/// <summary>
/// Helper service responsible for fetching the current spectator game state
/// and triggering its broadcast via the IGameStateBroadcaster abstraction.
/// </summary>
public class GameStateUpdateService 
{
    private readonly IMediator _mediator;
    private readonly IGameStateBroadcaster _gameStateBroadcaster;
    private readonly ILogger<GameStateUpdateService> _logger;

    public GameStateUpdateService(
        IMediator mediator,
        IGameStateBroadcaster gameStateBroadcaster,
        ILogger<GameStateUpdateService> logger)
    {
        _mediator = mediator;
        _gameStateBroadcaster = gameStateBroadcaster;
        _logger = logger;
    }

    /// <summary>
    /// Fetches the spectator state for the given game ID and broadcasts it.
    /// </summary>
    /// <param name="gameId">The ID of the game to update.</param>
    /// <param name="triggeringEvent">The domain event that triggered the update (for logging).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task FetchAndBroadcastStateAsync(Guid gameId, IDomainEvent triggeringEvent, CancellationToken cancellationToken)
    {
        if (gameId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to broadcast state for an empty GameId triggered by {EventType}.", triggeringEvent.GetType().Name);
            return;
        }

        _logger.LogInformation("Game event {EventType} occurred for Game {GameId}. Fetching and broadcasting spectator state.",
            triggeringEvent.GetType().Name, gameId);

        try
        {
            // Fetch the latest spectator state for the affected game
            var spectatorState = await _mediator.Send(new GetSpectatorGameStateQuery(gameId), cancellationToken).ConfigureAwait(false);

            if (spectatorState != null)
            {
                // Use the abstracted service to broadcast the update
                await _gameStateBroadcaster.BroadcastSpectatorStateAsync(
                    gameId,
                    spectatorState,
                    cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully broadcast spectator state for Game {GameId}", gameId);
            }
            else
            {
                _logger.LogWarning("Could not retrieve spectator state for Game {GameId} after event {EventType}. State not broadcast.", gameId, triggeringEvent.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching or broadcasting spectator state for Game {GameId} after event {EventType}.", gameId, triggeringEvent.GetType().Name);
            // Decide if error should be propagated or just logged
        }
    }
}