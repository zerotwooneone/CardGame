using CardGame.Domain.Turn;

namespace CardGame.Domain.Game;

/// <summary>
/// Represents a specific Card instance within the game.
/// Includes both its functional type and an identifier for its appearance.
/// </summary>
public record Card // Using record for immutability and value equality
{
    /// <summary>
    /// Gets the functional type of the card (Guard, Priest, etc.).
    /// </summary>
    public CardType Type { get; }

    /// <summary>
    /// Gets an integer identifier for the card's specific appearance (e.g., 1, 2, 3).
    /// This allows multiple visual versions of functionally identical cards.
    /// </summary>
    public int AppearanceId { get; } // Changed from string to int

    // --- Static instance caching removed ---
    // Each card in the deck/hand is now a distinct instance.

    /// <summary>
    /// Initializes a new instance of the Card record.
    /// Constructor is now public or internal to allow creation of specific instances (e.g., when building the deck).
    /// </summary>
    /// <param name="type">The functional type of the card.</param>
    /// <param name="appearanceId">An integer identifier for the card's appearance.</param> // Updated param doc
    public Card(CardType type, int appearanceId) // Constructor parameter changed to int
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        // Removed null check for appearanceId as int cannot be null.
        // Add validation if needed (e.g., appearanceId >= 0)
        AppearanceId = appearanceId;
    }

    // --- GetInstance method removed ---

    /// <summary>
    /// Gets the rank of the card based on its type.
    /// </summary>
    public int Rank => Type.Value; // Delegate Rank access
}