using CardGame.Domain.Exceptions;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Turn;

namespace CardGame.Domain.Game;

/// <summary>
/// Represents a Player's hand of cards (usually 1, sometimes 2 briefly).
/// Implemented as an immutable Value Object using a record. Holds specific Card instances.
/// </summary>
public sealed record Hand // Changed from class to record
{
    public static Hand Empty { get; } = new Hand(Enumerable.Empty<Card>());

    public IReadOnlyList<Card> Cards { get; }

    private Hand(IEnumerable<Card> cards)
    {
        Cards = cards?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(cards));
    }

    public bool IsEmpty => !Cards.Any();
    public int Count => Cards.Count;

    /// <summary>
    /// Returns a *new* Hand instance with the specified card added.
    /// </summary>
    /// <exception cref="DomainException">Thrown if adding the card exceeds max hand size (e.g., > 2).</exception>
    public Hand Add(Card card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        if (Cards.Count >= 2) // Max hand size constraint
        {
            throw new HandFullException("Cannot add card, hand is full (max 2 cards).");
        }
        // Returns a new record instance
        return new Hand(Cards.Concat(new[] { card }));
    }

    /// <summary>
    /// Returns a *new* Hand instance with a card matching the specified type removed.
    /// If multiple cards of the same type exist (e.g., two Guards with different appearances),
    /// it removes the first one found.
    /// </summary>
    /// <exception cref="DomainException">Thrown if a card of the specified type is not found in the hand.</exception>
    public Hand Remove(Card cardInstance) // Changed parameter type
    {
        if (cardInstance == null) throw new ArgumentNullException(nameof(cardInstance));

        // Find the specific card instance to remove using record equality
        var cardToRemove = Cards.FirstOrDefault(c => c == cardInstance);
        if (cardToRemove == null)
        {
            // Or maybe throw a different exception if the specific instance isn't found
            throw new CardNotFoundInHandException(cardInstance.Type.Name);
        }
        // Return a new hand excluding the specific instance found
        return new Hand(Cards.Where(c => c != cardToRemove));
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
        // This logic remains the same, returns the specific Card instance
        return Cards.Count == 1 ? Cards.Single() : null;
    }

    /// <summary>
    /// Gets all card instances currently in the hand.
    /// </summary>
    public IEnumerable<Card> GetCards() => Cards;
}