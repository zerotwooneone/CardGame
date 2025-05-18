using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using System;
using System.Collections.Generic;

namespace CardGame.Domain.Providers;

/// <summary>
/// Provides a Cthulhu-themed deck configuration.
/// </summary>
public class CthulhuDeckProvider : IDeckProvider
{
    private const string CthulhuBackAppearanceId = "assets/decks/cthulu/back.webp";
    private const string DeckTheme = "cthulu";

    /// <inheritdoc />
    public Guid DeckId => new Guid("00000000-0000-0000-0000-000000000002");

    /// <summary>
    /// Gets the Cthulhu-themed deck.
    /// The deck composition is functionally similar to Love Letter:
    /// - 5 Investigators (Guard)
    /// - 2 Cultists (Priest)
    /// - 2 Researchers (Baron)
    /// - 2 Elder Signs (Handmaid)
    /// - 2 Necronomicons (Prince)
    /// - 1 Mad Arab (King)
    /// - 1 Shoggoth (Countess)
    /// - 1 Cthulhu (Princess)
    /// </summary>
    /// <returns>A <see cref="DeckDefinition"/> representing the Cthulhu deck.</returns>
    public DeckDefinition GetDeck()
    {
        var cards = new List<Card>();

        // Helper to add multiple cards of the same type with Cthulhu theme
        void AddCards(CardType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // AppearanceId uses the deck theme folder
                cards.Add(new Card($"assets/decks/{DeckTheme}/{type.Name.ToLowerInvariant()}.webp", type));
            }
        }

        AddCards(CardType.Guard, 5);      // Investigators
        AddCards(CardType.Priest, 2);     // Cultists
        AddCards(CardType.Baron, 2);      // Researchers
        AddCards(CardType.Handmaid, 2);   // Elder Signs
        AddCards(CardType.Prince, 2);     // Necronomicons
        AddCards(CardType.King, 1);       // Mad King
        AddCards(CardType.Countess, 1);   // Shoggoth
        AddCards(CardType.Princess, 1);   // Cthulhu

        return new DeckDefinition(cards, CthulhuBackAppearanceId);
    }
}
