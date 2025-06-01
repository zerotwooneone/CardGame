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
    /// Uses reference equality to ensure only the exact instance is removed.
    /// </summary>
    /// <exception cref="CardNotFoundInHandException">Thrown if the specific card instance is not found by reference.</exception>
    /// <param name="cardInstanceToRemove">The specific card instance to remove.</param>
    public Hand Remove(Card cardInstanceToRemove) // Parameter is Card instance
    {
        if (cardInstanceToRemove == null) throw new ArgumentNullException(nameof(cardInstanceToRemove));

        var newCards = new List<Card>(Cards.Count - 1); // Pre-allocate assuming one removal
        bool removed = false;
        foreach (var cardInHand in Cards)
        {
            if (!removed && ReferenceEquals(cardInHand, cardInstanceToRemove))
            {
                removed = true; // Skip this instance, only the first one found by reference
            }
            else
            {
                newCards.Add(cardInHand);
            }
        }

        if (!removed)
        {
            // This means the exact object reference passed in was not found in the hand.
            // This can happen if cardInstanceToRemove is a copy (equal by value) but not the actual reference from the hand.
            // Or if the card was already removed or never there.
            throw new CardNotFoundInHandException($"Specific instance of card '{cardInstanceToRemove.Type.Name}' (AppearanceId: '{cardInstanceToRemove.AppearanceId}') not found in hand by reference.");
        }

        return new Hand(newCards);
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