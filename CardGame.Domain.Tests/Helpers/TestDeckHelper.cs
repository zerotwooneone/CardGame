using CardGame.Domain.Game;
using CardGame.Domain.Types;

namespace CardGame.Domain.Tests.Helpers;

/// <summary>
/// Provides helper methods and constants for creating test data related to cards and decks.
/// </summary>
public static class TestDeckHelper
{
    // --- Hardcoded Guids for Test Cards ---
    public static readonly Guid CardId_G1 = new Guid("11111111-0000-0000-0000-000000000001");
    public static readonly Guid CardId_G2 = new Guid("11111111-0000-0000-0000-000000000002");
    public static readonly Guid CardId_G3 = new Guid("11111111-0000-0000-0000-000000000003");
    public static readonly Guid CardId_G4 = new Guid("11111111-0000-0000-0000-000000000004");
    public static readonly Guid CardId_G5 = new Guid("11111111-0000-0000-0000-000000000005");
    public static readonly Guid CardId_P1 = new Guid("22222222-0000-0000-0000-000000000001");
    public static readonly Guid CardId_P2 = new Guid("22222222-0000-0000-0000-000000000002");
    public static readonly Guid CardId_B1 = new Guid("33333333-0000-0000-0000-000000000001");
    public static readonly Guid CardId_B2 = new Guid("33333333-0000-0000-0000-000000000002");
    public static readonly Guid CardId_H1 = new Guid("44444444-0000-0000-0000-000000000001");
    public static readonly Guid CardId_H2 = new Guid("44444444-0000-0000-0000-000000000002");
    public static readonly Guid CardId_Pr1 = new Guid("55555555-0000-0000-0000-000000000001");
    public static readonly Guid CardId_Pr2 = new Guid("55555555-0000-0000-0000-000000000002");
    public static readonly Guid CardId_K = new Guid("66666666-0000-0000-0000-000000000001");
    public static readonly Guid CardId_C = new Guid("77777777-0000-0000-0000-000000000001");
    public static readonly Guid CardId_Pss = new Guid("88888888-0000-0000-0000-000000000001");
    // --- End Hardcoded Guids ---

    /// <summary>
    /// Creates the standard list of 16 Love Letter cards using predefined IDs for testing.
    /// The order matches the original implementation (Princess first, Guards last).
    /// </summary>
    public static List<Card> CreateStandardTestCardList()
    {
        return new List<Card> {
            new Card(CardId_Pss, CardType.Princess), new Card(CardId_C, CardType.Countess),
            new Card(CardId_K, CardType.King), new Card(CardId_Pr1, CardType.Prince),
            new Card(CardId_Pr2, CardType.Prince), new Card(CardId_H1, CardType.Handmaid),
            new Card(CardId_H2, CardType.Handmaid), new Card(CardId_B1, CardType.Baron),
            new Card(CardId_B2, CardType.Baron), new Card(CardId_P1, CardType.Priest),
            new Card(CardId_P2, CardType.Priest), new Card(CardId_G1, CardType.Guard),
            new Card(CardId_G2, CardType.Guard), new Card(CardId_G3, CardType.Guard),
            new Card(CardId_G4, CardType.Guard), new Card(CardId_G5, CardType.Guard),
        };
    }
}