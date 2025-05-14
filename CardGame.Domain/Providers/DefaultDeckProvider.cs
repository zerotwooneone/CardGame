using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame.Domain.Providers;

/// <summary>
/// Provides the default Love Letter deck configuration.
/// </summary>
public class DefaultDeckProvider : IDeckProvider
{
    /// <summary>
    /// Gets the default Love Letter deck.
    /// The deck composition is:
    /// - 5 Guards (Rank 1)
    /// - 2 Priests (Rank 2)
    /// - 2 Barons (Rank 3)
    /// - 2 Handmaids (Rank 4)
    /// - 2 Princes (Rank 5)
    /// - 1 King (Rank 6)
    /// - 1 Countess (Rank 7)
    /// - 1 Princess (Rank 8)
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Card"/> representing the default deck.</returns>
    public IEnumerable<Card> GetDeck()
    {
        var cards = new List<Card>();

        // Helper to add multiple cards of the same type
        void AddCards(CardType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // AppearanceId can be simple for now, e.g., TypeName-Index
                // This could be enhanced later for actual visual variants if needed.
                cards.Add(new Card($"{type.Name}-{i + 1}", type));
            }
        }

        AddCards(CardType.Guard, 5);
        AddCards(CardType.Priest, 2);
        AddCards(CardType.Baron, 2);
        AddCards(CardType.Handmaid, 2);
        AddCards(CardType.Prince, 2);
        AddCards(CardType.King, 1);
        AddCards(CardType.Countess, 1);
        AddCards(CardType.Princess, 1);

        return cards.OrderBy(c => Guid.NewGuid()).ToList(); // Shuffle the deck
    }
}
