using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using System;
using System.Collections.Generic;

namespace CardGame.Domain.Providers;

/// <summary>
/// Provides the default Love Letter deck configuration.
/// </summary>
public class DefaultDeckProvider : BaseDeckProvider
{
    public override Guid DeckId => new Guid("00000000-0000-0000-0000-000000000001");
    
    public override string DisplayName => "Standard Love Letter";
    
    public override string Description => "The original Love Letter deck with 16 cards and 8 different character types.";
    
    protected override string ThemeName => "default";
    
    protected override string DeckBackAppearanceId => "assets/decks/default/back.webp";

    protected override IEnumerable<CardQuantity> GetCardQuantities()
    {
        return new List<CardQuantity>
        {
            new(CardType.Guard, 5),
            new(CardType.Priest, 2),
            new(CardType.Baron, 2),
            new(CardType.Handmaid, 2),
            new(CardType.Prince, 2),
            new(CardType.King, 1),
            new(CardType.Countess, 1),
            new(CardType.Princess, 1)
        };
    }

    // All card effect logic (PerformCardEffect and Execute[CardName]Effect methods)
    // is now inherited from BaseDeckProvider.
    // DefaultDeckProvider only needs to define its unique properties and card quantities.
}
