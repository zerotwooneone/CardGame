using System.Collections.Immutable;
using CardGame.Domain.Exceptions;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game;
/// <summary>
/// Represents the deck of cards in a Love Letter game round.
/// Implemented as an immutable Value Object using a record.
/// </summary>
public sealed record Deck 
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

    // Private constructor to enforce creation via factory and ensure immutability
    private Deck(ImmutableStack<Card> cards)
    {
        _cards = cards ?? throw new ArgumentNullException(nameof(cards));
    }

    /// <summary>
    /// Creates a standard 16-card Love Letter deck, shuffled.
    /// </summary>
    /// <param name="randomizer">Optional randomizer for shuffling (defaults to System.Random). Inject for testability.</param>
    /// <returns>A new, shuffled Deck instance.</returns>
    public static Deck CreateShuffledDeck(Common.IRandomizer? randomizer = null)
    {
        var rand = randomizer ?? new Common.DefaultRandomizer();
        var standardCards = CreateStandardCardList();

        // Shuffle the list in place
        rand.Shuffle(standardCards);

        // Create an immutable stack from the shuffled list
        var cardStack = ImmutableStack.CreateRange(standardCards);

        return new Deck(cardStack);
    }

    /// <summary>
    /// Draws the top card from the deck.
    /// </summary>
    /// <returns>A tuple containing the drawn Card and the new Deck instance with the card removed.</returns>
    /// <exception cref="DomainException">Thrown if the deck is empty.</exception>
    public (Card DrawnCard, Deck RemainingDeck) Draw()
    {
        if (IsEmpty)
        {
            throw new EmptyDeckException("Cannot draw card from an empty deck.");
        }

        // Pop returns the item and the modified stack (immutable)
        var remainingStack = _cards.Pop(out Card drawnCard);

        // Create a new Deck record instance with the remaining stack
        var remainingDeck = new Deck(remainingStack);

        return (drawnCard, remainingDeck);
    }

    /// <summary>
    /// Creates the standard list of 16 Love Letter cards with types and basic appearance IDs.
    /// </summary>
    private static List<Card> CreateStandardCardList()
    {
        return new List<Card>
        {
            new( Guid.NewGuid(), CardType.Princess),
            new( Guid.NewGuid(),CardType.Countess),
            new(Guid.NewGuid(),CardType.King),
            new(Guid.NewGuid(),CardType.Prince),
            new(Guid.NewGuid(),CardType.Prince),
            new(Guid.NewGuid(),CardType.Handmaid),
            new(Guid.NewGuid(),CardType.Handmaid),
            new(Guid.NewGuid(),CardType.Baron),
            new(Guid.NewGuid(),CardType.Baron),
            new(Guid.NewGuid(),CardType.Priest),
            new(Guid.NewGuid(),CardType.Priest),
            new(Guid.NewGuid(),CardType.Guard),
            new(Guid.NewGuid(),CardType.Guard),
            new(Guid.NewGuid(),CardType.Guard),
            new(Guid.NewGuid(),CardType.Guard),
            new(Guid.NewGuid(),CardType.Guard),
        };
    }
    
    /// <summary>
    /// Rehydrates a Deck instance from a persisted sequence of cards.
    /// Assumes the provided sequence is in the correct stack order (top card is first).
    /// </summary>
    public static Deck Load(IEnumerable<Card> orderedCards)
    {
        // CreateRange expects the item that should be on top to be last in the IEnumerable
        // So, we might need to reverse the loaded order depending on how it was saved.
        // Assuming orderedCards is saved such that first element is top card:
        var stack = ImmutableStack.CreateRange(orderedCards.Reverse());
        return new Deck(stack); // Use private constructor
    }
}