using CardGame.Application.DTOs;
using CardGame.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CardGame.Application.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/Game
public class GameController : ControllerBase
{
    private readonly IMediator _mediator;

    public GameController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets the public spectator view for a specific game.
    /// Does not include secret information like player hands or deck order.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <returns>The spectator game state or Not Found.</returns>
    [HttpGet("{gameId}", Name = "GetSpectatorGameState")] // Route: GET /api/Game/{gameId}/spectator
    [ProducesResponseType(typeof(SpectatorGameStateDto), 200)] // OK
    [ProducesResponseType(404)] // Not Found
    public async Task<ActionResult<SpectatorGameStateDto>> GetSpectatorView(Guid gameId)
    {
        var query = new GetSpectatorGameStateQuery(gameId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Game with ID {gameId} not found.");
        }

        return Ok(result);
    }

    // --- Other endpoints for playing the game, etc. would go here ---
    // e.g., [HttpPost("{gameId}/play")]
    // e.g., [HttpPost("new")]

}