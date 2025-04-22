using CardGame.Application.Common.Models;

namespace CardGame.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for managing user-to-player ID mappings.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets the User model (PlayerId and Username) associated with the given username.
    /// If the username does not exist, a new User is generated, stored, and returned.
    /// Handles storage limits.
    /// </summary>
    /// <param name="username">The username to look up or add.</param>
    /// <returns>The User model containing the existing or newly generated Player ID and username.</returns>
    Task<User> GetOrAddUserAsync(string username);
    
    /// <summary>
    /// Attempts to find the User model for a given Player ID.
    /// </summary>
    /// <param name="playerId">The Player ID to look up.</param>
    /// <returns>The User model if found, otherwise null.</returns>
    Task<User?> GetUserByIdAsync(Guid playerId);
}