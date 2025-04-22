namespace CardGame.Application.DTOs;

/// <summary>
/// Represents information about a player within the PlayerGameStateDto.
/// Hand card details are omitted here; count is shown for all players.
/// </summary>
public class PlayerHandInfoDto // Renamed from SpectatorPlayerDto for clarity
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int HandCardCount { get; set; } // Show count for all players
    public List<string> PlayedCardTypes { get; set; } = new List<string>();
    public int TokensWon { get; set; }
    public bool IsProtected { get; set; }
}