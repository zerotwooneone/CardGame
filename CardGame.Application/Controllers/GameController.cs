using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using CardGame.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardGame.Application.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/Game
public class GameController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserAuthenticationService _authService; // Inject the auth service

    public GameController(IMediator mediator, IUserAuthenticationService authService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Gets the public spectator view for a specific game.
    /// Does not include secret information like player hands or deck order.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <returns>The spectator game state or Not Found.</returns>
    [HttpGet("{gameId}", Name = "GetSpectatorGameState")] // Route: GET /api/Game/{gameId}
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

    /// <summary>
    /// Gets the game state from the perspective of a specific player, including their hand.
    /// Requires the authenticated user to match the requested player ID.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <param name="playerId">The ID of the player whose state is requested.</param>
    /// <returns>The player-specific game state or Not Found / Forbidden.</returns>
    [HttpGet("{gameId}/players/{playerId}", Name = "GetPlayerGameState")]
    [Authorize] // Add this when using real authentication middleware
    [ProducesResponseType(typeof(PlayerGameStateDto), 200)] // OK
    [ProducesResponseType(401)] // Unauthorized (if real auth fails)
    [ProducesResponseType(403)] // Forbidden (if user tries to access other player's state)
    [ProducesResponseType(404)] // Not Found (if game or player in game not found)
    public async Task<ActionResult<PlayerGameStateDto>> GetPlayerState(Guid gameId, Guid playerId)
    {
        // 1. Get the ID of the user making the request (using fake service here)
        Guid currentUserId = User.Claims.FirstOrDefault(c => c.Type == "PlayerId")?.Value == null ? Guid.Empty : Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "PlayerId")?.Value);

        // --- Authorization Check ---
        // Ensure the authenticated user is requesting their own state.
        // PlayerId from the URL must match the ID of the user identified by the auth token/cookie.
        if (currentUserId == Guid.Empty || currentUserId != playerId)
        {
            // If using real auth, middleware might return 401 Unauthorized before this.
            // If user is authenticated but requesting wrong player ID, return 403 Forbidden.
            return Forbid(); // Or return Unauthorized() if user wasn't identified at all
        }
        // --- End Authorization Check ---


        // 2. Send the query to get the player-specific state
        var query = new GetPlayerGameStateQuery(gameId, playerId); // Pass player ID from URL
        var result = await _mediator.Send(query);

        // 3. Handle results
        if (result == null)
        {
            // Could be Game not found, or Player not found within the game
            return NotFound($"Game with ID {gameId} not found, or Player with ID {playerId} not found in this game.");
        }

        return Ok(result);
    }

}