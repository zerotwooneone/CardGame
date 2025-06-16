using CardGame.Domain.Game;
using CardGame.Domain.Game.Event;
using CardGame.Domain.Types;
using System;

namespace CardGame.Domain.Interfaces;

/// <summary>
/// Defines operations that can be performed on a game by external components like deck providers.
/// This interface provides a controlled way to modify game state while maintaining encapsulation.
/// </summary>
public interface IGameOperations
{
    /// <summary>
    /// Adds a log entry to the game log.
    /// </summary>
    /// <param name="entry">The log entry to add</param>
    void AddLogEntry(GameLogEntry entry);

    /// <summary>
    /// Eliminates a player from the game.
    /// </summary>
    /// <param name="playerId">ID of the player to eliminate</param>
    /// <param name="reason">The reason for the elimination</param>
    /// <param name="cardResponsible">Optional card that caused the elimination, if any</param>
    /// <exception cref="InvalidOperationException">Thrown if the player is protected or already eliminated</exception>
    void EliminatePlayer(Guid playerId, string reason, Card? cardResponsible = null);

    /// <summary>
    /// Gives a card to a player.
    /// </summary>
    /// <param name="playerId">ID of the player to receive the card</param>
    /// <param name="card">The card to give</param>
    /// <exception cref="InvalidOperationException">Thrown if the player cannot receive cards</exception>
    void GiveCardToPlayer(Guid playerId, Card card);

    /// <summary>
    /// Draws a card from the deck and gives it to a player.
    /// </summary>
    /// <param name="playerId">ID of the player who should receive the card</param>
    /// <returns>The drawn card, or null if the deck is empty</returns>
    Card? DrawCardForPlayer(Guid playerId);

    /// <summary>
    /// Swaps the hands of two players.
    /// </summary>
    /// <param name="player1Id">ID of the first player</param>
    /// <param name="player2Id">ID of the second player</param>
    /// <exception cref="InvalidOperationException">Thrown if either player is eliminated or protected</exception>
    void SwapPlayerHands(Guid player1Id, Guid player2Id);

    /// <summary>
    /// Gets a player by ID.
    /// </summary>
    /// <param name="playerId">ID of the player to get</param>
    /// <returns>The player, or null if not found</returns>
    Player? GetPlayer(Guid playerId);
        
    /// <summary>
    /// Gets the current game state information needed by card effects.
    /// </summary>
    IGameStateInfo GetGameState();

    /// <summary>
    /// Awards one or more affection tokens to a specific player.
    /// </summary>
    /// <param name="playerId">The ID of the player to award tokens to.</param>
    /// <param name="tokensToAward">The number of tokens to award.</param>
    void AwardAffectionToken(Guid playerId, int tokensToAward);

    // --- Generic Deck Status Management ---

    /// <summary>
    /// Sets a custom, deck-specific status on a player.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="deckId">The unique ID of the deck setting the status.</param>
    /// <param name="statusKey">The key for the status (e.g., "JesterTokenHolder").</param>
    /// <param name="statusValue">The value for the status (e.g., the ID of the player who gave the token).</param>
    void SetPlayerDeckStatus(Guid playerId, Guid deckId, string statusKey, string? statusValue);

    /// <summary>
    /// Retrieves a custom, deck-specific status from a player.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="deckId">The unique ID of the deck that set the status.</param>
    /// <param name="statusKey">The key for the status.</param>
    /// <returns>The status value, or null if not set.</returns>
    string? GetPlayerDeckStatus(Guid playerId, Guid deckId, string statusKey);

    /// <summary>
    /// Clears a single custom, deck-specific status from a player.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="deckId">The unique ID of the deck that set the status.</param>
    /// <param name="statusKey">The key for the status to clear.</param>
    void ClearPlayerDeckStatus(Guid playerId, Guid deckId, string statusKey);

    /// <summary>
    /// Clears all deck-specific statuses set by a particular deck for a given player.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="deckId">The unique ID of the deck whose statuses should be cleared.</param>
    void ClearAllPlayerDeckStatusesForPlayer(Guid playerId, Guid deckId);

    /// <summary>
    /// Sets a custom, deck-specific status on the game itself.
    /// </summary>
    /// <param name="deckId">The unique ID of the deck setting the status.</param>
    /// <param name="statusKey">The key for the status (e.g., "SycophantMandateActive").</param>
    /// <param name="statusValue">The value for the status.</param>
    void SetGameDeckStatus(Guid deckId, string statusKey, string? statusValue);

    /// <summary>
    /// Retrieves a custom, deck-specific status from the game.
    /// </summary>
    /// <param name="deckId">The unique ID of the deck that set the status.</param>
    /// <param name="statusKey">The key for the status.</param>
    /// <returns>The status value, or null if not set.</returns>
    string? GetGameDeckStatus(Guid deckId, string statusKey);

    /// <summary>
    /// Clears a single custom, deck-specific status from the game.
    /// </summary>
    /// <param name="deckId">The unique ID of the deck that set the status.</param>
    /// <param name="statusKey">The key for the status to clear.</param>
    void ClearGameDeckStatus(Guid deckId, string statusKey);
}

/// <summary>
/// Provides read-only access to game state information needed by card effects.
/// </summary>
public interface IGameStateInfo
{
    /// <summary>
    /// Gets the number of cards remaining in the deck.
    /// </summary>
    int CardsRemaining { get; }
        
    /// <summary>
    /// Gets the current round number.
    /// </summary>
    int RoundNumber { get; }
        
    /// <summary>
    /// Gets the ID of the player whose turn it is.
    /// </summary>
    Guid CurrentTurnPlayerId { get; }
        
    /// <summary>
    /// Gets the list of players in the game.
    /// </summary>
    IReadOnlyList<Player> Players { get; }
}
