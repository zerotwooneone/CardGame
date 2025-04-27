using CardGame.Domain.Exceptions;
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

    private Hand(IEnumerable<Card> cards)
    {
        Cards = cards?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(cards));
    }

    /// <summary>
    /// Creates a Hand instance from a collection of cards (e.g., loaded from persistence).
    /// </summary>
    public static Hand Load(IEnumerable<Card> cards)
    {
        // Use private constructor. Handles null/empty via constructor logic.
        return new Hand(cards ?? Enumerable.Empty<Card>());
    }

    public bool IsEmpty => !Cards.Any();
    public int Count => Cards.Count;

    /// <summary>
    /// Returns a *new* Hand instance with the specified card added.
    /// </summary>
    /// <exception cref="HandFullException">Thrown if adding the card exceeds max hand size (e.g., > 2).</exception>
    public Hand Add(Card card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        if (Cards.Count >= 2) // Max hand size constraint
        {
            throw new HandFullException();
        }

        // Returns a new record instance
        return new Hand(Cards.Concat(new[] {card}));
    }

    /// <summary>
    /// Returns a *new* Hand instance with the specific card instance removed.
    /// Uses explicit IsSameCard check for clarity.
    /// </summary>
    /// <exception cref="CardNotFoundInHandException">Thrown if the specific card instance is not found.</exception>
    /// <param name="cardInstance">The specific card instance to remove.</param>
    public Hand Remove(Card cardInstance) // Parameter is Card instance
    {
        if (cardInstance == null) throw new ArgumentNullException(nameof(cardInstance));

        // Find the specific card instance using the explicit helper method
        var cardToRemove = Cards.FirstOrDefault(c => c.IsSameCard(cardInstance)); // Use IsSameCard
        if (cardToRemove == null)
        {
            throw new CardNotFoundInHandException(cardInstance.Type.Name);
        }

        // Return a new hand excluding the specific instance found, using explicit helper
        return new Hand(Cards.Where(c => !c.IsSameCard(cardToRemove))); // Use IsSameCard
    }


    /// <summary>
    /// Checks if the hand contains a card of the specified type.
    /// </summary>
    public bool Contains(CardType cardType)
    {
        if (cardType == null) return false;
        return Cards.Any(c => c.Type == cardType);
    }

    /// <summary>
    /// Gets the single card held, assuming the hand contains exactly one card.
    /// Returns null if the hand is empty or has more than one card.
    /// </summary>
    public Card? GetHeldCard()
    {
        return Cards.Count == 1 ? Cards.Single() : null;
    }

    /// <summary>
    /// Gets all card instances currently in the hand.
    /// </summary>
    public IEnumerable<Card> GetCards() => Cards;

    // Note: Record equality members are still generated based on all properties (Id, Type).
}