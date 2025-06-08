using CardGame.Domain.Providers;

namespace CardGame.Domain.BaseGame;

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
            new(CardRank.Guard, 5),
            new(CardRank.Priest, 2),
            new(CardRank.Baron, 2),
            new(CardRank.Handmaid, 2),
            new(CardRank.Prince, 2),
            new(CardRank.King, 1),
            new(CardRank.Countess, 1),
            new(CardRank.Princess, 1)
        };
    }

    // All card effect logic (PerformCardEffect and Execute[CardName]Effect methods)
    // is now inherited from BaseDeckProvider.
    // DefaultDeckProvider only needs to define its unique properties and card quantities.
}
