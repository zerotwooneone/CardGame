using CardGame.Application.Commands;
using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using CardGame.Application.Queries;
using CardGame.Domain.Exceptions;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardGame.Application.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/Game
public class GameController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGameRepository _gameRepository; // Inject repository to look up card instance

    public GameController(IMediator mediator, IGameRepository gameRepository)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository)); // Inject repo
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

    /// <summary>
    /// Creates a new Love Letter game.
    /// </summary>
    /// <param name="request">Details for the new game, including player IDs.</param> // Updated param doc
    /// <returns>The ID of the newly created game.</returns>
    [HttpPost(Name = "CreateGame")] // Route: POST /api/Game
    [Authorize] // Require authentication
    [ProducesResponseType(typeof(Guid), 201)] // Created
    [ProducesResponseType(400)] // Bad Request (validation errors)
    [ProducesResponseType(401)] // Unauthorized
    public async Task<ActionResult<Guid>> CreateGame([FromBody] CreateGameRequestDto request) // Uses updated DTO
    {
        // Get the Player ID of the user making the request from their claims
        Guid creatorPlayerId = GetCurrentPlayerIdFromClaims();
        if (creatorPlayerId == Guid.Empty)
        {
            return Unauthorized("Could not identify creating user.");
        }

        // Basic check for distinct IDs in request before sending command
        if (request.PlayerIds == null || request.PlayerIds.Distinct().Count() != request.PlayerIds.Count)
        {
            return BadRequest("Player IDs must be provided and unique.");
        }

        if (!request.PlayerIds.Contains(creatorPlayerId))
        {
            return BadRequest("Creator must be a player in the game."); 
        }

        try
        {
            // Create command using PlayerIds from the request DTO
            var command = new CreateGameCommand(
                request.PlayerIds, // Pass the list of Guids
                creatorPlayerId,
                request.TokensToWin
            );

            var gameId = await _mediator.Send(command);

            return CreatedAtRoute("GetSpectatorGameState", new {gameId = gameId}, gameId);
        }
        catch (FluentValidation.ValidationException ex) // Catch validation errors from handler/pipeline
        {
            var errors = ex.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new ValidationProblemDetails(errors));
        }
        catch (Exception ex) // Catch other potential errors
        {
            // Log the exception ex
            return BadRequest(new ProblemDetails {Title = "Failed to create game.", Detail = ex.Message});
        }
    }
    
    /// <summary>
    /// Allows the authenticated player to play a card from their hand.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <param name="request">Details of the card play action.</param>
    /// <returns>Ok on success, or appropriate error status.</returns>
    [HttpPost("{gameId}/play", Name = "PlayCard")]
    [Authorize] // Require authentication
    [ProducesResponseType(200)] // OK
    [ProducesResponseType(400)] // Bad Request (validation, invalid move)
    [ProducesResponseType(401)] // Unauthorized
    [ProducesResponseType(403)] // Forbidden (wrong player)
    [ProducesResponseType(404)] // Not Found (game)
    public async Task<IActionResult> PlayCard(Guid gameId, [FromBody] PlayCardRequestDto request)
    {
        // 1. Get authenticated player ID
        Guid currentPlayerId = GetCurrentPlayerIdFromClaims();
        if (currentPlayerId == Guid.Empty)
        {
            return Unauthorized();
        }

        // 2. Preliminary check: Find the card instance in the player's hand
        // This requires loading the game state here in the controller, which isn't ideal CQRS.
        // Alternative: Pass CardId to the handler and let the handler load the game and find the card.
        // Let's go with the alternative for better separation.

        // 3. Parse GuessedCardType string into CardType object
        CardType? guessedType = null;
        if (!string.IsNullOrEmpty(request.GuessedCardType))
        {
            try
            {
                // Assumes CardType is an EnumLike generated class with FromName
                guessedType = CardType.FromName(request.GuessedCardType, ignoreCase: true);
            }
            catch (Exception) // Catch potential exception from FromName if invalid
            {
                return BadRequest($"Invalid value provided for GuessedCardType: '{request.GuessedCardType}'.");
            }
        }

        try
        {
            // **Refined Approach:** Handler needs Card instance, not just ID.
            // Controller needs to load game, find card, then send command.
            // This slightly breaks pure CQRS but is necessary here.
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) return NotFound($"Game {gameId} not found.");

            var player = game.Players.FirstOrDefault(p => p.Id == currentPlayerId);
            if (player == null) return Forbid("Authenticated user is not a player in this game."); // Or NotFound

            // Find the specific card instance in the player's hand by ID
            var cardToPlayInstance = player.Hand.GetCards().FirstOrDefault(c => c.Id == request.CardId);
            if (cardToPlayInstance == null)
            {
                return BadRequest($"Card with ID {request.CardId} not found in player's hand.");
            }

            // Now create the command with the actual Card instance
            var command = new PlayCardCommand(
                gameId,
                currentPlayerId,
                cardToPlayInstance, 
                request.TargetPlayerId,
                guessedType
            );

            // 4. Send the command
            await _mediator.Send(command);

            // 5. Return success
            return Ok(); // Simple 200 OK for successful command execution
        }
        catch (ValidationException ex) // Catch FluentValidation errors
        {
            return BadRequest(CreateValidationProblemDetails(ex));
        }
        catch (InvalidOperationException ex) // Catch fail-fast errors from handler
        {
             // Log ex
             return BadRequest(new ProblemDetails { Title = "Invalid Operation", Detail = ex.Message });
        }
        catch (DomainException ex) // Catch domain rule violations from aggregate
        {
             // Log ex
             // Return BadRequest with domain error code and message
             return BadRequest(new ProblemDetails { Title = "Game Rule Violation", Detail = ex.Message, Extensions = { { "errorCode", ex.ErrorCode } } });
        }
        catch (KeyNotFoundException ex) // Catch game not found from handler
        {
             return NotFound(new ProblemDetails{ Title = "Not Found", Detail = ex.Message });
        }
        catch (Exception ex) // Catch unexpected errors
        {
             // Log ex
             // Return a generic 500 Internal Server Error
             return StatusCode(500, new ProblemDetails { Title = "An unexpected error occurred while playing the card."});
        }
    }

    // --- Helper to get Player ID from Claims ---
    private Guid GetCurrentPlayerIdFromClaims()
    {
        var playerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PlayerId"); // Use custom claim
        if (playerIdClaim != null && Guid.TryParse(playerIdClaim.Value, out Guid playerId))
        {
            return playerId;
        }

        return Guid.Empty;
    }
    
    private ValidationProblemDetails CreateValidationProblemDetails(ValidationException ex)
    {
        var errors = ex.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return new ValidationProblemDetails(errors);
    }
}