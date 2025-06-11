using CardGame.Domain.Game;
using System;
using System.Collections.Generic;
using CardGame.Domain.Types;

namespace CardGame.Domain.Interfaces;

/// <summary>
/// Defines a contract for classes that provide deck configurations and card behaviors.
/// </summary>
public interface IDeckProvider
{
    /// <summary>
    /// Gets the unique identifier for the deck provided by this provider.
    /// </summary>
    Guid DeckId { get; }
    
    /// <summary>
    /// Gets the display name of the deck.
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Gets the description of the deck.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the collection of cards that make up a deck.
    /// </summary>
    /// <returns>A <see cref="DeckDefinition"/>.</returns>
    DeckDefinition GetDeck();
    
    /// <summary>
    /// Gets the definitions for the ranks used in the deck.
    /// </summary>
    IReadOnlyDictionary<int, IEnumerable<RankDefinition>> RankDefinitions { get; }

    /// <summary>
    /// Executes the effect of a played card.
    /// </summary>
    /// <param name="game">The game operations interface to interact with the game state.</param>
    /// <param name="actingPlayer">The ID of the player who played the card.</param>
    /// <param name="cardRankValue">The rank value of the card that was played.</param>
    /// <param name="targetPlayerId">The ID of the player targeted by the card, if any.</param>
    /// <param name="guessedRankValue">The rank value guessed by the acting player, if applicable.</param>
    void ExecuteCardEffect(
        IGameOperations game, 
        Player actingPlayer, 
        Card card,
        Player? targetPlayer, 
        int? guessedRankValue);
    
    
}
