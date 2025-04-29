


using CardGame.Domain.Types;

namespace CardGame.Domain.Game;

/// <summary>
/// Represents a specific Card instance within the game.
/// Includes a unique ID and its functional type. Behaves as a Value Object via record equality.
/// </summary>
public record Card // Using record for immutability and value equality
{
    /// <summary>
    /// Gets the unique identifier for this specific card instance.
    /// While the core game logic primarily uses 'Type', this 'Id' allows
    /// consumers (like the UI or specific game variants) to track and
    /// differentiate individual card objects, potentially for appearance variations.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the functional type of the card (Guard, Priest, etc.).
    /// Initialized with null! to satisfy nullable reference type analysis;
    /// the constructor ensures a non-null value is assigned.
    /// </summary>
    public CardType Type { get; } = null!;

    /// <summary>
    /// Initializes a new instance of the Card record.
    /// </summary>
    /// <param name="id">The unique identifier for this card instance.</param>
    /// <param name="type">The functional type of the card.</param>
    public Card(Guid id, CardType type)
    {
        Id = id;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// Gets the rank of the card based on its type.
    /// </summary>
    public int Rank => Type.Value;
}