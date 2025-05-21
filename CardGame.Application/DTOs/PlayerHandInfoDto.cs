namespace CardGame.Application.DTOs;

/// <summary>
/// Represents information about a player within the PlayerGameStateDto.
/// Hand card details are omitted here; count is shown for all players.
/// </summary>
public class PlayerHandInfoDto 
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public int HandCardCount { get; set; } // Show count for all players
    public List<CardDto> PlayedCards { get; set; } = new List<CardDto>();
    public int TokensWon { get; set; }
    public bool IsProtected { get; set; }
}