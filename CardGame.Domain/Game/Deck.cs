using System.Collections.Immutable;
using CardGame.Domain.Common;
using CardGame.Domain.Exceptions;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game;

/// <summary>
/// Represents the deck of cards in a Love Letter game round.
/// Implemented as an immutable Value Object using a record.
/// Created from a specific list of cards provided externally.
/// </summary>
public sealed record Deck // Changed from class to record
{
    // Internal state uses ImmutableStack for LIFO behavior (drawing from top)
    private readonly ImmutableStack<Card> _cards;

    /// <summary>
    /// Gets the number of cards remaining in the deck.
    /// </summary>
    public int CardsRemaining => _cards.IsEmpty ? 0 : _cards.Count();

    /// <summary>
    /// Checks if the deck is empty.
    /// </summary>
    public bool IsEmpty => _cards.IsEmpty;

    // Private constructor for immutability and factory methods
    private Deck(ImmutableStack<Card> cards)
    {
        _cards = cards ?? throw new ArgumentNullException(nameof(cards));
    }

    // --- Factory Methods ---

    /// <summary>
    /// Creates a new Deck instance by shuffling a provided set of cards.
    /// </summary>
    /// <param name="cardSet">The complete list of cards to include in the deck.</param>
    /// <param name="randomizer">Optional randomizer for shuffling (defaults to System.Random). Inject for testability.</param>
    /// <returns>A new, shuffled Deck instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if cardSet is null.</exception>
    public static Deck CreateShuffled(IReadOnlyList<Card> cardSet, IRandomizer? randomizer = null) // Renamed factory
    {
        if (cardSet == null) throw new ArgumentNullException(nameof(cardSet));

        var rand = randomizer ?? new DefaultRandomizer();
        // Create a mutable copy to shuffle
        var mutableCardList = cardSet.ToList();

        // Shuffle the list in place
        rand.Shuffle(mutableCardList);

        // Create an immutable stack from the shuffled list
        // CreateRange expects the item that should be on top to be last in the IEnumerable
        var cardStack = ImmutableStack.CreateRange(mutableCardList);

        return new Deck(cardStack); // Use private constructor
    }

    /// <summary>
    /// Rehydrates a Deck instance from a persisted sequence of cards.
    /// Assumes the provided sequence is in the correct stack order (top card is first).
    /// </summary>
    public static Deck Load(IEnumerable<Card> orderedCards)
    {
        // CreateRange expects the item that should be on top to be last in the IEnumerable
        // So, we reverse the loaded order assuming first element is top card.
        var stack = ImmutableStack.CreateRange(orderedCards?.Reverse() ?? Enumerable.Empty<Card>());
        return new Deck(stack); // Use private constructor
    }
    // --- End Factory Methods ---


    /// <summary>
    /// Draws the top card from the deck.
    /// </summary>
    /// <returns>A tuple containing the drawn Card and the new Deck instance with the card removed.</returns>
    /// <exception cref="EmptyDeckException">Thrown if the deck is empty.</exception>
    public (Card DrawnCard, Deck RemainingDeck) Draw()
    {
        if (IsEmpty)
        {
            throw new EmptyDeckException();
        }

        var remainingStack = _cards.Pop(out Card drawnCard);
        var remainingDeck = new Deck(remainingStack); // Create new instance
        return (drawnCard, remainingDeck);
    }

    // --- CreateStandardCardList method removed ---
}