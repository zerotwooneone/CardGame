namespace CardGame.Application.Common.Models;

/// <summary>
/// Represents basic user information linking a username to a player ID.
/// </summary>
/// <param name="PlayerId">The unique identifier for the player in the game context.</param>
/// <param name="Username">The user's login name.</param>
public record User(
    Guid PlayerId,
    string Username);