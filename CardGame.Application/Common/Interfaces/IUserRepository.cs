namespace CardGame.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for managing user-to-player ID mappings.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets the Player ID associated with the given username.
    /// If the username does not exist, a new Player ID is generated,
    /// stored, and returned. Handles storage limits.
    /// </summary>
    /// <param name="username">The username to look up or add.</param>
    /// <returns>The existing or newly generated Player ID for the username.</returns>
    Task<Guid> GetOrAddPlayerIdAsync(string username);

    /// <summary>
    /// Attempts to find the Player ID for a given username without adding it.
    /// </summary>
    /// <param name="username">The username to look up.</param>
    /// <returns>The Player ID if found, otherwise null.</returns>
    Task<Guid?> FindPlayerIdByUsernameAsync(string username);
}