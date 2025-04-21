


using CardGame.Domain.Types;

namespace CardGame.Domain.Game;

/// <summary>
    /// Represents a specific Card instance within the game.
    /// Includes a unique ID and its functional type. Behaves as a Value Object via record equality.
    /// Provides explicit comparison methods for clarity.
    /// </summary>
    public class Card // Using record for immutability and value equality
    {
        /// <summary>
        /// Gets the unique identifier for this specific card instance.
        /// </summary>
        public Guid Id { get; } // Added unique ID

        /// <summary>
        /// Gets the functional type of the card (Guard, Priest, etc.).
        /// Initialized with null! to satisfy nullable reference type analysis;
        /// the constructor ensures a non-null value is assigned.
        /// </summary>
        public CardType Type { get; } = null!; // Keep null! fix

        // --- AppearanceId property removed ---

        // --- Static instance caching removed ---

        /// <summary>
        /// Initializes a new instance of the Card record.
        /// Constructor is now public or internal to allow creation of specific instances (e.g., when building the deck).
        /// </summary>
        /// <param name="id">The unique identifier for this card instance.</param> // Added id param
        /// <param name="type">The functional type of the card.</param>
        public Card(Guid id, CardType type) // Updated constructor signature
        {
            Id = id; // Assign Id
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        // --- GetInstance method removed ---

        /// <summary>
        /// Gets the rank of the card based on its type.
        /// </summary>
        public int Rank => Type.Value; // Delegate Rank access

        // --- Added Helper Methods ---

        /// <summary>
        /// Checks if this card instance is the same as another card instance based on their unique IDs.
        /// </summary>
        /// <param name="other">The card to compare with.</param>
        /// <returns>True if both cards are not null and have the same Id; otherwise, false.</returns>
        public bool IsSameCard(Card? other)
        {
            // Explicitly compare by Id for clarity where instance identity matters.
            return other != null && this.Id == other.Id;
        }

        /// <summary>
        /// Checks if this card has the same rank as another card.
        /// </summary>
        /// <param name="other">The card to compare with.</param>
        /// <returns>True if both cards are not null and have the same Rank; otherwise, false.</returns>
        public bool IsSameRank(Card? other)
        {
            // Explicitly compare by Rank for clarity where only rank matters.
            return other != null && this.Rank == other.Rank;
        }
    }