using System.Collections.Concurrent;
using CardGame.Application.Common.Interfaces;

namespace CardGame.Infrastructure.Persistance;

/// <summary>
    /// In-memory implementation of IUserRepository for testing/development.
    /// Stores username-to-PlayerId mappings with a fixed capacity, overwriting the oldest entry when full.
    /// </summary>
    public class InMemoryUserRepository : IUserRepository
    {
        private const int MaxCapacity = 1000; // Set the maximum number of users to store

        // Stores the mapping: username -> PlayerId
        private readonly ConcurrentDictionary<string, Guid> _userMappings =
            new ConcurrentDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase); // Case-insensitive usernames

        // Tracks the order of insertion to manage capacity
        private readonly ConcurrentQueue<string> _userQueue = new ConcurrentQueue<string>();

        // Lock object for thread safety during add/remove operations across dictionary and queue
        private readonly object _lock = new object();

        /// <summary>
        /// Gets the Player ID for a username, adding it if it doesn't exist.
        /// Handles capacity limits by removing the oldest entry if necessary.
        /// </summary>
        public Task<Guid> GetOrAddPlayerIdAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            // Check if user already exists (case-insensitive)
            if (_userMappings.TryGetValue(username, out Guid existingPlayerId))
            {
                // Optional: Could update the user's position in the queue if LRU behavior is desired,
                // but for simple overwrite-oldest, just return the existing ID.
                return Task.FromResult(existingPlayerId);
            }

            // User doesn't exist, need to add them. Use lock for consistency.
            lock (_lock)
            {
                // Double-check inside lock in case another thread added it
                if (_userMappings.TryGetValue(username, out Guid raceConditionPlayerId))
                {
                    return Task.FromResult(raceConditionPlayerId);
                }

                // Check capacity BEFORE adding
                while (_userQueue.Count >= MaxCapacity)
                {
                    // Dequeue the oldest username
                    if (_userQueue.TryDequeue(out string? oldestUsername))
                    {
                        // Remove the corresponding entry from the dictionary
                        if (oldestUsername != null)
                        {
                            _userMappings.TryRemove(oldestUsername, out _);
                        }
                    }
                    else
                    {
                        // Should not happen if count >= MaxCapacity, but break defensively
                        break;
                    }
                }

                // Generate a new deterministic Player ID for the new user
                Guid newPlayerId = GenerateDeterministicGuid(username);

                // Add to dictionary and queue
                if (_userMappings.TryAdd(username, newPlayerId))
                {
                    _userQueue.Enqueue(username);
                    return Task.FromResult(newPlayerId);
                }
                else
                {
                    // Should theoretically not happen due to double-check, but handle defensively
                    // If TryAdd failed, it means it was added between the check and TryAdd. Get the existing one.
                    return Task.FromResult(_userMappings[username]);
                }
            }
        }

        /// <summary>
        /// Attempts to find the Player ID for a given username without adding it.
        /// </summary>
        public Task<Guid?> FindPlayerIdByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Task.FromResult<Guid?>(null);
            }

            if (_userMappings.TryGetValue(username, out Guid existingPlayerId))
            {
                return Task.FromResult<Guid?>(existingPlayerId);
            }

            return Task.FromResult<Guid?>(null);
        }


        // Helper to generate a predictable Guid from a string (same as in AuthController)
        // Consider moving this to a shared helper class if used elsewhere.
        private static Guid GenerateDeterministicGuid(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return new Guid(hash);
            }
        }
    }