using CardGame.Domain.Game;
using System.Collections.Generic;

namespace CardGame.Domain.Interfaces;

/// <summary>
/// Defines a contract for providing a deck of cards.
/// </summary>
public interface IDeckProvider
{
    /// <summary>
    /// Gets the collection of cards that make up a deck.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Card"/>.</returns>
    IEnumerable<Card> GetDeck();
}
