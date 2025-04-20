using System.Collections.Immutable;
using CardGame.Domain.Exceptions;
using CardGame.Domain.Game.GameException;
using CardGame.Domain.Turn;

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
        // Appearance IDs assigned simply (0/1 for pairs, 0 for singles)
        // Assumes Card constructor is Card(CardType type, int appearanceId)
        // Assumes CardType has static members Guard, Priest, etc.
        return new List<Card>
        {
            new Card(CardType.Princess, 0),
            new Card(CardType.Countess, 0),
            new Card(CardType.King, 0),
            new Card(CardType.Prince, 0),
            new Card(CardType.Prince, 1),
            new Card(CardType.Handmaid, 0),
            new Card(CardType.Handmaid, 1),
            new Card(CardType.Baron, 0),
            new Card(CardType.Baron, 1),
            new Card(CardType.Priest, 0),
            new Card(CardType.Priest, 1),
            new Card(CardType.Guard, 0),
            new Card(CardType.Guard, 1),
            new Card(CardType.Guard, 2),
            new Card(CardType.Guard, 3),
            new Card(CardType.Guard, 4),
        };
    }

    // Note: By changing to 'record', C# automatically generates:
    // - IEquatable<Deck> implementation
    // - override Equals(object obj)
    // - override GetHashCode()
    // - operator == and operator !=
    // These compare based on the record's fields (in this case, the _cards stack).
    // Default record equality for collections/stacks typically compares references,
    // not deep content equality. For Love Letter, this is usually sufficient as
    // we primarily care about the deck's state transitions (drawing cards).
}