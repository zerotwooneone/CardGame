namespace CardGame.Application.DTOs;

/// <summary>
/// Represents basic public information about a card (e.g., for the discard pile).
/// </summary>
public class CardDto
{
    public int Rank { get; set; } 
    public string AppearanceId { get; set; }
}