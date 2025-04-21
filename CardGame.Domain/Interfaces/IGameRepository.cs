namespace CardGame.Domain.Interfaces;

/// <summary>
/// Defines the repository contract for accessing and persisting Game aggregates.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Retrieves a Game aggregate by its unique identifier.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The found Game aggregate, or null if not found.</returns>
    Task<Game.Game?> GetByIdAsync(Guid gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the state of a Game aggregate.
    /// This typically handles both creating new games and updating existing ones (Upsert).
    /// Alternatively, you could define separate AddAsync and UpdateAsync methods.
    /// </summary>
    /// <param name="game">The Game aggregate to save.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAsync(Game.Game game, CancellationToken cancellationToken = default);

    // Optional: Add other methods if needed, e.g.,
    // Task<IEnumerable<Game>> FindActiveGamesAsync(CancellationToken cancellationToken = default);
    // Task DeleteAsync(Guid gameId, CancellationToken cancellationToken = default);
}