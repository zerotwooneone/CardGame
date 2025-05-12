using CardGame.Domain.Game;
using CardGame.Domain.Types;

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
            new(CardType.Princess.Name, CardType.Princess), 
            new(CardType.Countess.Name, CardType.Countess),
            new(CardType.King.Name, CardType.King), 
            new(CardType.Prince.Name, CardType.Prince),
            new(CardType.Prince.Name, CardType.Prince), 
            new(CardType.Handmaid.Name, CardType.Handmaid),
            new(CardType.Handmaid.Name, CardType.Handmaid), 
            new(CardType.Baron.Name, CardType.Baron),
            new(CardType.Baron.Name, CardType.Baron), 
            new(CardType.Priest.Name, CardType.Priest),
            new(CardType.Priest.Name, CardType.Priest), 
            new(CardType.Guard.Name, CardType.Guard),
            new(CardType.Guard.Name, CardType.Guard), 
            new(CardType.Guard.Name, CardType.Guard),
            new(CardType.Guard.Name, CardType.Guard), 
            new(CardType.Guard.Name, CardType.Guard),
        };
    }
}