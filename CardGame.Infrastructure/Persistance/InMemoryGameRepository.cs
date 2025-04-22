using System.Collections.Concurrent;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Infrastructure.Persistance;

/// <summary>
/// In-memory implementation of the IGameRepository for testing and development.
/// Limits the number of stored games, overwriting the oldest when capacity is reached.
/// </summary>
public class InMemoryGameRepository : IGameRepository
{
    private const int MaxCapacity = 1000; // Set the maximum number of games to store

    // Stores the games: gameId -> Game instance
    // Made static so the repository behaves like a persistent store across instances (common for in-memory singleton)
    private static readonly ConcurrentDictionary<Guid, Game> _games = new ConcurrentDictionary<Guid, Game>();

    // Tracks the order of insertion to manage capacity
    private static readonly ConcurrentQueue<Guid> _gameQueue = new ConcurrentQueue<Guid>();

    // Lock object for thread safety during add/remove operations across dictionary and queue
    private static readonly object _lock = new object();

    // --- Removed PreLoadedGameId constant ---
    // --- Removed static constructor ---
    // --- Removed InitializePreLoadedGame method ---
    // --- Removed CreateStandardCardListForLoad method ---

    // Internal synchronous save method used by SaveAsync
    private static void SaveInternal(Game game)
    {
         if (game == null) throw new ArgumentNullException(nameof(game));

         lock (_lock)
         {
             bool isUpdate = _games.ContainsKey(game.Id);

             // If it's a new game, check capacity
             if (!isUpdate)
             {
                 while (_gameQueue.Count >= MaxCapacity)
                 {
                     if (_gameQueue.TryDequeue(out Guid oldestGameId))
                     {
                         _games.TryRemove(oldestGameId, out _);
                     }
                     else { break; } // Should not happen
                 }
             }

             // Add or update the game
             _games.AddOrUpdate(game.Id, game, (id, existingGame) => game);

             // If it was a new game, add its ID to the queue
             if (!isUpdate)
             {
                 _gameQueue.Enqueue(game.Id);
             }
         }
    }


    // --- IGameRepository Implementation ---

    public Task<Game?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        // Simulate async operation
        // await Task.Yield(); // Not strictly needed for ConcurrentDictionary reads

        _games.TryGetValue(gameId, out var game);
        // Consider returning a clone if true isolation is needed, but reference is simpler for basic in-mem.
        return Task.FromResult(game);
    }

    public Task SaveAsync(Game game, CancellationToken cancellationToken = default)
    {
        // Use internal synchronous method for logic, wrap in Task.Run for async signature
        // Or just call SaveInternal and return Task.CompletedTask if true async simulation isn't needed
        SaveInternal(game);
        return Task.CompletedTask;

        // If true async simulation is desired:
        // return Task.Run(() => SaveInternal(game), cancellationToken);
    }
}