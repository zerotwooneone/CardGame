using CardGame.Domain.Game.GameException;

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
    /// Returns a *new* Hand instance with the first occurrence of a card matching
    /// the specified cardInstance (by AppearanceId and Type) removed.
    /// </summary>
    /// <exception cref="CardNotFoundInHandException">Thrown if no card matching the criteria is found.</exception>
    /// <param name="cardToRemoveByValue">The card instance whose value (AppearanceId and Type) should be matched and removed.</param>
    public Hand Remove(Card cardToRemoveByValue) 
    {
        if (cardToRemoveByValue == null) throw new ArgumentNullException(nameof(cardToRemoveByValue));

        int indexToRemove = -1;
        for (int i = 0; i < Cards.Count; i++)
        {
            // Use default record equality for Card (compares AppearanceId and Type)
            if (Cards[i] == cardToRemoveByValue) 
            {
                indexToRemove = i;
                break; // Found the first match
            }
        }

        if (indexToRemove == -1)
        {
            throw new CardNotFoundInHandException($"Card '{cardToRemoveByValue.Rank}' (AppearanceId: '{cardToRemoveByValue.AppearanceId}') not found in hand by value.");
        }

        var newCardsList = Cards.ToList(); // Create a mutable copy
        newCardsList.RemoveAt(indexToRemove); // Remove the item at the found index

        return new Hand(newCardsList);
    }

    /// <summary>
    /// Gets the single card held, assuming the hand contains exactly one card.
    /// Returns the card if Cards.Count == 1; otherwise, returns null (if hand is empty or contains more than one card).
    /// </summary>
    public Card? GetHeldCard() => Cards.Count == 1 ? Cards.Single() : null;

    /// <summary>
    /// Gets all card instances currently in the hand.
    /// </summary>
    public IEnumerable<Card> GetCards() => Cards;

    // Note: Record equality members are generated based on all properties (Id, Type).
}