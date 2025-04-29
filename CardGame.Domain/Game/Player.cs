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
    public Hand Hand { get; private set; } = Hand.Empty;
    public List<CardType> PlayedCards { get; private set; } = new List<CardType>();
    public int TokensWon { get; private set; } = 0;
    public bool IsProtected { get; private set; } = false;

    // Private constructor enforce creation via factory or aggregate root
    private Player(Guid id, string name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    // --- Factory Methods ---

    /// <summary>
    /// Factory method to create a new player instance.
    /// </summary>
    internal static Player Create(string name) // Consider if this should take ID too
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be empty.", nameof(name));
        return new Player(Guid.NewGuid(), name); // Use private constructor
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
        player.Status = status ?? PlayerStatus.Active;
        player.Hand = hand ?? Hand.Empty;
        player.PlayedCards = playedCards ?? new List<CardType>();
        player.TokensWon = tokensWon;
        player.IsProtected = isProtected;
        return player;
    }
    // --- End Factory Methods ---


    // --- Internal Methods for Aggregate ---
    internal void PrepareForNewRound()
    {
        Status = PlayerStatus.Active;
        Hand = Hand.Empty;
        PlayedCards.Clear();
        IsProtected = false;
    }

    internal void GiveCard(Card card)
    {
        Hand = Hand.Add(card);
    }

    internal void PlayCard(Card cardInstance)
    {
        Hand = Hand.Remove(cardInstance);
        PlayedCards.Add(cardInstance.Type);
    }

    internal Card? DiscardHand(bool deckEmpty)
    {
        var card = Hand.GetHeldCard();
        Hand = Hand.Empty;
        if (card != null) PlayedCards.Add(card.Type);
        return card;
    } // Simplified

    internal void SwapHandWith(Player otherPlayer)
    {
        var tmp = this.Hand;
        this.Hand = otherPlayer.Hand;
        otherPlayer.Hand = tmp;
    } // Simplified

    internal void SetProtection(bool isProtected)
    {
        this.IsProtected = isProtected;
    }

    internal void Eliminate()
    {
        Status = PlayerStatus.Eliminated;
    }

    internal void AddToken()
    {
        TokensWon++;
    }
}