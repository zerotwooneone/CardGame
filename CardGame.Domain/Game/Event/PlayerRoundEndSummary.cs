namespace CardGame.Domain.Game.Event;

public record PlayerRoundEndSummary(
    Guid PlayerId,
    string PlayerName, // Include name for convenience
    Card? FinalHeldCard, // The actual card instance held at end (null if eliminated/empty)
    List<int> DiscardPileValues, // List of card values (ranks) discarded
    int TokensWon // Current token count
);