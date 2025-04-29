using CardGame.Domain.Game.GameException;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game;

/// <summary>
/// Represents a Player's hand of cards (usually 1, sometimes 2 briefly).
/// Implemented as an immutable Value Object using a record. Holds specific Card instances.
/// </summary>
public sealed record Hand 
{
    public static Hand Empty { get; } = new Hand(Enumerable.Empty<Card>());

    public IReadOnlyList<Card> Cards { get; }

    // Explicit private constructor is kept to maintain initialization logic
    private Hand(IEnumerable<Card> cards)
    {
        Cards = cards?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(cards));
    }

    // --- Factory Method for Rehydration ---
    /// <summary>
    /// Creates a Hand instance from a collection of cards (e.g., loaded from persistence).
    /// </summary>
    public static Hand Load(IEnumerable<Card> cards)
    {
        return new Hand(cards ?? Enumerable.Empty<Card>());
    }
    // --- End Factory Method ---


    public bool IsEmpty => !Cards.Any();
    public int Count => Cards.Count;

    /// <summary>
    /// Returns a *new* Hand instance with the specified card added.
    /// </summary>
    public Hand Add(Card card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        if (Cards.Count >= 2) throw new HandFullException();
        return new Hand(Cards.Concat(new[] {card}));
    }

    /// <summary>
    /// Returns a *new* Hand instance with the specific card instance removed.
    /// Uses default record equality (comparing Id and Type).
    /// </summary>
    /// <exception cref="CardNotFoundInHandException">Thrown if the specific card instance is not found.</exception>
    /// <param name="cardInstance">The specific card instance to remove.</param>
    public Hand Remove(Card cardInstance) // Parameter is Card instance
    {
        if (cardInstance == null) throw new ArgumentNullException(nameof(cardInstance));

        // Find the specific card instance using default record equality (checks Id and Type)
        var cardToRemove = Cards.FirstOrDefault(c => c == cardInstance); // Use default equality
        if (cardToRemove == null)
        {
            // Card instance provided wasn't found in the hand
            throw new CardNotFoundInHandException(cardInstance.Type.Name);
        }

        // Return a new hand excluding the specific instance found
        return new Hand(Cards.Where(c => c != cardToRemove)); // Use default inequality
    }


    /// <summary>
    /// Checks if the hand contains a card of the specified type.
    /// </summary>
    public bool Contains(CardType cardType) => cardType != null && Cards.Any(c => c.Type == cardType);

    /// <summary>
    /// Gets the single card held, assuming the hand contains exactly one card.
    /// </summary>
    public Card? GetHeldCard() => Cards.Count == 1 ? Cards.Single() : null;

    /// <summary>
    /// Gets all card instances currently in the hand.
    /// </summary>
    public IEnumerable<Card> GetCards() => Cards;

    // Note: Record equality members are generated based on all properties (Id, Type).
}