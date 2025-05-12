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
    /// The cards held by the player at the end of the round.
    /// </summary>
    public List<CardDto> CardsHeld { get; set; } = new List<CardDto>();

    /// <summary>
    /// The total number of tokens of affection the player has after this round.
    /// </summary>
    public int TokensWon { get; set; }
}