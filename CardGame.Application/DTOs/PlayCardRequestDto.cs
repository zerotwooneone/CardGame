using System.ComponentModel.DataAnnotations;

namespace CardGame.Application.DTOs;

public class PlayCardRequestDto
{
    /// <summary>
    /// The unique ID of the specific card instance being played from the player's hand.
    /// </summary>
    [Required]
    public string CardId { get; set; } = string.Empty;

    /// <summary>
    /// Optional: The ID of the player being targeted by the card effect (if applicable).
    /// </summary>
    public Guid? TargetPlayerId { get; set; }

    /// <summary>
    /// Optional: The type of card being guessed (used only when playing a Guard).
    /// Represented as an integer (rank value).
    /// </summary>
    public int? GuessedCardType { get; set; } // Use string for simplicity in DTO
}