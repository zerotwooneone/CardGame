using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using CardGame.Domain.Game.GameException; 
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame.Domain.Providers;

/// <summary>
/// Provides a Cthulhu-themed deck configuration.
/// </summary>
public class CthulhuDeckProvider : BaseDeckProvider 
{
    // DeckId, DisplayName, Description, ThemeName, DeckBackAppearanceId implemented via overrides
    public override Guid DeckId => new Guid("00000000-0000-0000-0000-000000000002");
    public override string DisplayName => "Cthulhu Mythos";
    public override string Description => "A Love Letter variant with a Cthulhu Mythos theme.";
    protected override string ThemeName => "cthulu";
    protected override string DeckBackAppearanceId => "assets/decks/cthulu/back.webp";

    /// <summary>
    /// Gets the Cthulhu-themed deck card quantities.
    /// Functionally similar to Love Letter:
    /// - 5 Investigators (Guard)
    /// - 2 Cultists (Priest)
    /// - 2 Researchers (Baron)
    /// - 2 Elder Signs (Handmaid)
    /// - 2 Necronomicons (Prince)
    /// - 1 Mad Arab (King)
    /// - 1 Shoggoth (Countess)
    /// - 1 Cthulhu (Princess)
    /// </summary>
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
}
