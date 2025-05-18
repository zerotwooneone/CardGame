namespace CardGame.Application.DTOs;

/// <summary>
/// Represents the publicly visible state of a player for spectators.
/// </summary>
public class SpectatorPlayerDto
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; } 
    public int HandCardCount { get; set; } // Count only, not the actual cards
    public List<CardDto> PlayedCards { get; set; } = new List<CardDto>(); // Show cards played this round
    public int TokensWon { get; set; }
    public bool IsProtected { get; set; } // Handmaid status is public
}