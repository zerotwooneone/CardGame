namespace CardGame.Application.DTOs;

/// <summary>
/// Represents basic public information about a card (e.g., for the discard pile).
/// </summary>
public class CardDto
{
    public int Type { get; set; } 
    public string Id { get; set; }
}