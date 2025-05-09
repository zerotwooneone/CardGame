namespace CardGame.Application.DTOs;

/// <summary>
/// Represents the overall summary sent to clients when a round ends.
/// This is sent to the client.
/// </summary>
public class RoundEndSummaryDto
{
    /// <summary>
    /// The reason the round ended (e.g., "Last player standing", "Deck empty, highest card wins").
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the player who won the round.
    /// Null if the round was a draw or had no winner.
    /// </summary>
    public Guid? WinnerPlayerId { get; set; }

    /// <summary>
    /// A list containing the summary details for each player at the end of the round.
    /// </summary>
    public List<RoundEndPlayerSummaryDto> PlayerSummaries { get; set; } = new List<RoundEndPlayerSummaryDto>();
}