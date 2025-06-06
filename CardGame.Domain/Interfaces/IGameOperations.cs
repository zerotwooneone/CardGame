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
