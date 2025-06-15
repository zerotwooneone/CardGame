using CardGame.Domain.Types;

namespace CardGame.Domain.Game
{
    /// <summary>
    /// Represents a card in the game, defined by its rank value and appearance ID.
    /// This record is immutable and provides value equality.
    /// </summary>
    /// <param name="RankValue">The numerical value representing the card's rank.</param>
    /// <param name="AppearanceId">The identifier for the card's visual appearance.</param>
    public record Card(int Rank, string AppearanceId);
}