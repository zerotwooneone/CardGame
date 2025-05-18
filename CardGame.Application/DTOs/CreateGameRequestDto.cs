using System.ComponentModel.DataAnnotations;

namespace CardGame.Application.DTOs;

public class CreateGameRequestDto
{
    /// <summary>
    /// List of unique Player IDs for the players joining the game (including the creator).
    /// Must contain between 2 and 4 unique Player IDs.
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "At least 2 players are required.")]
    [MaxLength(4, ErrorMessage = "No more than 4 players are allowed.")]
    public List<Guid> PlayerIds { get; set; } = new List<Guid>(); // Changed from PlayerUsernames

    /// <summary>
    /// The ID of the deck to be used for this game.
    /// </summary>
    [Required(ErrorMessage = "Deck ID must be provided.")]
    public Guid DeckId { get; set; }

    /// <summary>
    /// Optional: Number of tokens needed to win the game. Defaults to standard rules.
    /// </summary>
    [Range(1, 10, ErrorMessage = "Tokens to win must be between 1 and 10.")] // Example range
    public int? TokensToWin { get; set; } // Nullable for optional override
}