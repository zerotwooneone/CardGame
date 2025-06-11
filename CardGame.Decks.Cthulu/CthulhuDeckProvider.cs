using CardGame.Decks.BaseGame;
using CardGame.Domain.Types;

namespace CardGame.Decks.Cthulu;

/// <summary>
/// Provides a Cthulhu-themed deck configuration.
/// </summary>
public class CthulhuDeckProvider : DefaultDeckProvider 
{
    // DeckId, DisplayName, Description, ThemeName, DeckBackAppearanceId implemented via overrides
    public override Guid DeckId => new Guid("00000000-0000-0000-0000-000000000002");
    public override string DisplayName => "Cthulhu Mythos";
    public override string Description => "A Love Letter variant with a Cthulhu Mythos re-skin.";
    protected override string ThemeName => "cthulu";
    protected override string DeckBackAppearanceId => "assets/decks/cthulu/back.webp";

    /// <summary>
    /// Gets the appearance ID for a given card type, based on the theme.
    /// Derived classes can override for more complex appearance logic.
    /// </summary>
    protected override string GetCardAppearanceId(RankDefinition rank, int index)
    {
        var cardRank = CardRank.FromValue(rank.Value);
        return $"assets/decks/cthulu/{cardRank.Name.ToLowerInvariant()}.webp";
    }
}
