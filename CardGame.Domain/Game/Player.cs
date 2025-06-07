using CardGame.Domain.Types;
using Microsoft.Extensions.Logging;
using CardGame.Domain.Interfaces;

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
    public List<CardRank> PlayedCards { get; private set; } = new List<CardRank>();
    public int TokensWon { get; private set; } = 0;
    public bool IsProtected { get; private set; } = false;
    public bool IsPlayersTurn { get; set; } = false;

    private readonly ILogger<Player> _logger;

    // Private constructor enforce creation via factory or aggregate root
    private Player(Guid id, string name, ILogger<Player> logger)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _logger = logger;
    }

    // --- Factory Methods ---

    /// <summary>
    /// Factory method to create a new player instance.
    /// </summary>
    internal static Player Create(string name, ILogger<Player> logger) // Consider if this should take ID too
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be empty.", nameof(name));
        return new Player(Guid.NewGuid(), name, logger); // Use private constructor
    }

    /// <summary>
    /// Rehydrates a Player entity from its persisted state.
    /// </summary>
    public static Player Load(
        Guid id,
        string name,
        PlayerStatus status,
        Hand hand, // Assume Hand VO is already loaded
        List<CardRank> playedCards, // Assume List<CardType> is loaded
        int tokensWon,
        bool isProtected,
        ILogger<Player> logger)
    {
        var player = new Player(id, name, logger); // Use private constructor
        player.Status = status ?? PlayerStatus.Active;
        player.Hand = hand ?? Hand.Empty;
        player.PlayedCards = playedCards ?? new List<CardRank>();
        player.TokensWon = tokensWon;
        player.IsProtected = isProtected;
        return player;
    }
    // --- End Factory Methods ---


    // --- Public Methods ---
    public bool CanDrawCard() // gameContext might be needed for more complex rules later
    {
        // In Love Letter, a player draws if they are active and have 0 or 1 card.
        // They draw to 2, then play one down to 1.
        return Status == PlayerStatus.Active && Hand.Cards.Count < 2;
    }

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
        _logger.LogDebug("Player {PlayerName} ({PlayerId}) hand BEFORE playing {CardType} ({CardInstanceId}): {HandCards}", 
            Name, Id, cardInstance.Rank.Name, cardInstance.AppearanceId.Substring(0,4), 
            string.Join(", ", Hand.Cards.Select(c => $"{c.Rank.Name}({c.AppearanceId.Substring(0,4)})")));

        Hand = Hand.Remove(cardInstance); // This is PlayerHand.Remove which calls ImmutableList<Card>.Remove
        PlayedCards.Add(cardInstance.Rank);

        _logger.LogDebug("Player {PlayerName} ({PlayerId}) hand AFTER playing {CardType} ({CardInstanceId}): {HandCards}", 
            Name, Id, cardInstance.Rank.Name, cardInstance.AppearanceId.Substring(0,4), 
            string.Join(", ", Hand.Cards.Select(c => $"{c.Rank.Name}({c.AppearanceId.Substring(0,4)})")));
    }

    internal Card? DiscardHand(bool deckEmpty)
    {
        if (Hand.Cards.Count == 0) return null; // Nothing to discard

        var cardsInHandBeforeDiscard = Hand.GetCards().ToList(); // Store all cards
        Hand = Hand.Empty; // Discard the hand

        Card? princessCard = null;
        foreach (var cardInHand in cardsInHandBeforeDiscard)
        {
            PlayedCards.Add(cardInHand.Rank); // Add all discarded cards to played cards
            if (cardInHand.Rank == CardGame.Domain.Types.CardRank.Princess) 
            {
                princessCard = cardInHand; // Note if Princess was discarded
            }
        }
        
        return princessCard; // Return Princess if discarded, otherwise null
    }

    internal void SwapHandWith(Player otherPlayer)
    {
        (Hand, otherPlayer.Hand) = (otherPlayer.Hand, Hand);
    } // Simplified

    internal void SetProtection(bool isProtected)
    {
        IsProtected = isProtected;
    }

    internal void Eliminate()
    {
        Status = PlayerStatus.Eliminated;
    }

    internal void AddToken()
    {
        TokensWon++;
    }

    // Method to reset player state for a new round
    internal void StartNewRound(ILogger<Player> logger) // Added logger parameter to match Game.cs call, though _logger field exists
    {
        // _logger = logger; // Could re-assign if intended, or use existing _logger if constructor injects it properly.
        Hand = Hand.Empty; 
        Status = PlayerStatus.Active;
        IsProtected = false;
        IsPlayersTurn = false;
        // PlayedCards.Clear(); // Typically, played cards history is for the whole game, not per round. Confirm if this should be cleared.
        _logger.LogDebug("Player {PlayerName} ({PlayerId}) state reset for new round.", Name, Id);
    }
}