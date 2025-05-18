using CardGame.Domain.Game;
using System;

namespace CardGame.Domain.Interfaces;

/// <summary>
/// Defines a contract for classes that provide deck configurations.
/// </summary>
public interface IDeckProvider
{
    /// <summary>
    /// Gets the unique identifier for the deck provided by this provider.
    /// </summary>
    Guid DeckId { get; }

    /// <summary>
    /// Gets the collection of cards that make up a deck.
    /// </summary>
    /// <returns>A <see cref="DeckDefinition"/>.</returns>
    DeckDefinition GetDeck();
}
