using CardGame.Domain.Game;
using CardGame.Domain.Types;
using CardRank = CardGame.Domain.BaseGame.CardRank;

namespace CardGame.Domain.Tests.Helpers;

/// <summary>
/// Provides helper methods and constants for creating test data related to cards and decks.
/// </summary>
public static class TestDeckHelper
{
    /// <summary>
    /// Creates the standard list of 16 Love Letter cards using predefined IDs for testing.
    /// The order matches the original implementation (Princess first, Guards last).
    /// </summary>
    public static List<Card> CreateStandardTestCardList()
    {
        return new List<Card> {
            new(CardRank.Princess.Name, CardRank.Princess), 
            new(CardRank.Countess.Name, CardRank.Countess),
            new(CardRank.King.Name, CardRank.King), 
            new(CardRank.Prince.Name, CardRank.Prince),
            new(CardRank.Prince.Name, CardRank.Prince), 
            new(CardRank.Handmaid.Name, CardRank.Handmaid),
            new(CardRank.Handmaid.Name, CardRank.Handmaid), 
            new(CardRank.Baron.Name, CardRank.Baron),
            new(CardRank.Baron.Name, CardRank.Baron), 
            new(CardRank.Priest.Name, CardRank.Priest),
            new(CardRank.Priest.Name, CardRank.Priest), 
            new(CardRank.Guard.Name, CardRank.Guard),
            new(CardRank.Guard.Name, CardRank.Guard), 
            new(CardRank.Guard.Name, CardRank.Guard),
            new(CardRank.Guard.Name, CardRank.Guard), 
            new(CardRank.Guard.Name, CardRank.Guard),
        };
    }
}