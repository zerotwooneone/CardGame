using CardGame.Application.DTOs;
using MediatR;

namespace CardGame.Application.Queries;

/// <summary>
    /// Query to retrieve the game state from a specific player's perspective.
    /// </summary>
    /// <param name="GameId">The ID of the game.</param>
    /// <param name="RequestingPlayerId">The ID of the player requesting their state.</param>
    public record GetPlayerGameStateQuery(Guid GameId, Guid RequestingPlayerId) : IRequest<PlayerGameStateDto?>;