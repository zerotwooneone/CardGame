namespace CardGame.Domain.Game.Event;

public record PlayerRoundEndSummary(
    Guid PlayerId,
    string PlayerName, // Include name for convenience
    List<Card> CardsHeld, // Changed from Card? FinalHeldCard to List<Card> CardsHeld
    List<int> DiscardPileValues, // List of card values (ranks) discarded
    int TokensWon // Current token count
);