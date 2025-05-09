namespace CardGame.Application.DTOs;

/// <summary>
/// Represents the summary details for a single player at the end of a round.
/// This is sent to the client.
/// </summary>
public class RoundEndPlayerSummaryDto
{
    /// <summary>
    /// The ID of the player.
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// The name of the player.
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// The card held by the player at the end of the round (if any).
    /// Null if the player was eliminated or had no card.
    /// </summary>
    public CardDto? FinalHeldCard { get; set; } // Uses the existing CardDto

    /// <summary>
    /// The list of card values (ranks) in the player's discard pile for this round.
    /// </summary>
    public List<int> DiscardPileValues { get; set; } = new List<int>();

    /// <summary>
    /// The total number of tokens of affection the player has after this round.
    /// </summary>
    public int TokensWon { get; set; }
}