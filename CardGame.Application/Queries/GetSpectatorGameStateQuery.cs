using CardGame.Application.DTOs;
using MediatR;

namespace CardGame.Application.Queries;

/// <summary>
    /// Query to retrieve the spectator view of a specific game.
    /// </summary>
    /// <param name="GameId">The ID of the game to retrieve.</param>
    public record GetSpectatorGameStateQuery(Guid GameId) : IRequest<SpectatorGameStateDto?>;