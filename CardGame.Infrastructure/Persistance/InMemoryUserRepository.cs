using System.Collections.Concurrent;
using CardGame.Application.Common.Interfaces;
using CardGame.Application.Common.Models;

namespace CardGame.Infrastructure.Persistance;

/// <summary>
/// In-memory implementation of IUserRepository for testing/development.
/// Stores User models (PlayerId, Username) with a fixed capacity,
/// overwriting the oldest entry when full.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private const int MaxCapacity = 1000; // Set the maximum number of users to store

    // Stores the mapping: username -> User object
    private static readonly ConcurrentDictionary<string, User> _usersByUsername =
        new ConcurrentDictionary<string, User>(StringComparer.OrdinalIgnoreCase); // Case-insensitive usernames

    // Stores the mapping: PlayerId -> User object (for faster ID lookup)
    private static readonly ConcurrentDictionary<Guid, User> _usersById =
        new ConcurrentDictionary<Guid, User>();

    // Tracks the order of insertion (by username) to manage capacity
    private static readonly ConcurrentQueue<string> _userQueue = new ConcurrentQueue<string>();

    // Lock object for thread safety during add/remove operations across structures
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets the User model for a username, adding it if it doesn't exist.
    /// Handles capacity limits by removing the oldest entry if necessary.
    /// </summary>
    public Task<User> GetOrAddUserAsync(string username) // Updated method signature
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        // Check if user already exists (case-insensitive)
        if (_usersByUsername.TryGetValue(username, out User? existingUser))
        {
            return Task.FromResult(existingUser);
        }

        // User doesn't exist, need to add them. Use lock for consistency.
        lock (_lock)
        {
            // Double-check inside lock
            if (_usersByUsername.TryGetValue(username, out User? raceConditionUser))
            {
                return Task.FromResult(raceConditionUser);
            }

            // Check capacity BEFORE adding
            while (_userQueue.Count >= MaxCapacity)
            {
                if (_userQueue.TryDequeue(out string? oldestUsername))
                {
                    if (oldestUsername != null && _usersByUsername.TryRemove(oldestUsername, out User? removedUser))
                    {
                        // Also remove from the ID lookup dictionary
                        if(removedUser != null) _usersById.TryRemove(removedUser.PlayerId, out _);
                    }
                }
                else { break; } // Defensive break
            }

            // Generate a new deterministic Player ID
            Guid newPlayerId = GenerateDeterministicGuid(username);
            var newUser = new User(newPlayerId, username);

            // Add to both dictionaries and the queue
            if (_usersByUsername.TryAdd(username, newUser) && _usersById.TryAdd(newPlayerId, newUser))
            {
                _userQueue.Enqueue(username);
                return Task.FromResult(newUser);
            }
            else
            {
                // Handle potential failure (e.g., if TryAdd failed unexpectedly)
                // Rollback might be needed if one TryAdd succeeded but the other failed
                // For simplicity, assume it works or retrieve existing if already added by race condition
                if (_usersByUsername.TryGetValue(username, out User? addedUser))
                {
                    // Ensure it's also in the ID dictionary
                    _usersById.TryAdd(addedUser.PlayerId, addedUser);
                    return Task.FromResult(addedUser);
                }
                // If something went wrong, throw or return default?
                throw new InvalidOperationException($"Failed to add user '{username}' consistently.");
            }
        }
    }

    /// <summary>
    /// Attempts to find the User model for a given Player ID.
    /// </summary>
    public Task<User?> GetUserByIdAsync(Guid playerId) // New method implementation
    {
        if (playerId == Guid.Empty)
        {
            return Task.FromResult<User?>(null);
        }

        _usersById.TryGetValue(playerId, out User? user);
        return Task.FromResult(user);
    }

    // --- FindPlayerIdByUsernameAsync removed ---

    // Helper to generate a predictable Guid from a string
    private static Guid GenerateDeterministicGuid(string input)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
}