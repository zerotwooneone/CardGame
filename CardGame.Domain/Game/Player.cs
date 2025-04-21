using CardGame.Domain.Types;

namespace CardGame.Domain.Game;

/// <summary>
/// Represents a Player within the context of a Game aggregate.
/// </summary>
public class Player // Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public PlayerStatus Status { get; private set; } = PlayerStatus.Active;
    public Hand Hand { get; private set; } = Hand.Empty; // Player holds a Hand record instance now
    public List<CardType> PlayedCards { get; private set; } = new List<CardType>(); // Tracks types played this round
    public int TokensWon { get; private set; } = 0;
    public bool IsProtected { get; private set; } = false; // Protection status (Handmaid)

    // Private constructor enforce creation via factory or aggregate root
    private Player(Guid id, string name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Factory method to create a new player.
    /// </summary>
    internal static Player Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be empty.", nameof(name));

        return new Player(Guid.NewGuid(), name);
    }
    
    /// <summary>
    /// Rehydrates a Player entity from its persisted state.
    /// </summary>
    public static Player Load(
        Guid id,
        string name,
        PlayerStatus status,
        Hand hand, // Assume Hand VO is already loaded
        List<CardType> playedCards, // Assume List<CardType> is loaded
        int tokensWon,
        bool isProtected)
    {
        var player = new Player(id, name); // Use private constructor

        // Assign loaded state
        player.Status = status ?? PlayerStatus.Active; // Default if null? Or throw?
        player.Hand = hand ?? Hand.Empty;
        player.PlayedCards = playedCards ?? new List<CardType>();
        player.TokensWon = tokensWon;
        player.IsProtected = isProtected;

        return player;
    }

    /// <summary>
    /// Resets player state for the start of a new round.
    /// </summary>
    internal void PrepareForNewRound()
    {
        Status = PlayerStatus.Active;
        Hand = Hand.Empty; // Assigns the static Empty Hand record instance
        PlayedCards.Clear();
        IsProtected = false;
        // TokensWon persists across rounds
    }

    /// <summary>
    /// Gives a card instance to the player.
    /// </summary>
    internal void GiveCard(Card card) // Accepts a specific Card instance
    {
        Hand = Hand.Add(card); // Replace Hand with new immutable record instance
    }

    /// <summary>
    /// Marks a card type as played by the player for the current round.
    /// Removes the specified card instance from the hand and adds its type to the played cards list.
    /// </summary>
    /// <param name="cardInstance">The specific card instance being played.</param>
    internal void PlayCard(Card cardInstance) // Changed parameter type, returns void
    {
        if (cardInstance == null) throw new ArgumentNullException(nameof(cardInstance));
        // Call the updated Hand.Remove method with the specific instance
        this.Hand = Hand.Remove(cardInstance); // Update the player's hand state
        PlayedCards.Add(cardInstance.Type); // Still track played types
        // No longer needs to return the card instance
    }


    /// <summary>
    /// Discards the player's entire hand (usually one card instance).
    /// Adds the discarded card type(s) to the played cards list.
    /// Returns the primary card instance discarded (or null if hand was empty).
    /// </summary>
    internal Card? DiscardHand(bool deckEmpty) // deckEmpty might influence future logic
    {
        var cardToDiscard = Hand.GetHeldCard(); // Get the specific card instance before emptying
        if (cardToDiscard != null)
        {
             PlayedCards.Add(cardToDiscard.Type);
        }
        // Also add any other cards if hand somehow had more than one
        foreach(var card in Hand.GetCards().Where(c => c != cardToDiscard)) // Uses default record inequality here
        {
             PlayedCards.Add(card.Type);
        }

        Hand = Hand.Empty; // Replace Hand with new empty record instance
        return cardToDiscard; // Return the specific discarded card instance (or null)
    }

    /// <summary>
    /// Swaps hands (the Hand record instance) with another player.
    /// </summary>
    internal void SwapHandWith(Player otherPlayer)
    {
        if (otherPlayer == null) throw new ArgumentNullException(nameof(otherPlayer));
        if (otherPlayer == this) return; // Cannot swap with self

        // Direct swap of the Hand record instances
        (this.Hand, otherPlayer.Hand) = (otherPlayer.Hand, this.Hand);
    }

    /// <summary>
    /// Sets or clears the player's Handmaid protection status.
    /// </summary>
    internal void SetProtection(bool isProtected)
    {
        this.IsProtected = isProtected;
    }

    /// <summary>
    /// Marks the player as eliminated for the current round.
    /// </summary>
    internal void Eliminate()
    {
        Status = PlayerStatus.Eliminated;
        // Optionally clear hand or move to a "lost cards" pile if needed by rules
        // Hand = Hand.Empty;
    }

    /// <summary>
    /// Awards a token of affection to the player.
    /// </summary>
    internal void AddToken()
    {
        TokensWon++;
    }
}