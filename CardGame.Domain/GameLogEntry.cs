using CardGame.Domain.Types; // For CardType

namespace CardGame.Domain;

public class GameLogEntry
{
    public Guid Id { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public GameLogEventType EventType { get; private set; }
    public Guid ActingPlayerId { get; private set; }
    public string ActingPlayerName { get; private set; }
    public Guid? TargetPlayerId { get; private set; }
    public string? TargetPlayerName { get; private set; }
    public Guid? RevealedCardId { get; private set; }
    public CardType? RevealedCardType { get; private set; } // Existing, used for Priest/general reveal
    public bool IsPrivate { get; private set; }
    public string? Message { get; private set; }

    // --- New Structured Properties for Visual Story --- 
    public CardType? PlayedCardType { get; private set; }
    public int? PlayedCardValue { get; private set; }

    // For Guard Guess
    public CardType? GuessedCardType { get; private set; }
    public int? GuessedCardValue { get; private set; }
    public bool? WasGuessCorrect { get; private set; }

    // For Baron Comparison
    public CardType? Player1ComparedCardType { get; private set; } // ActingPlayer's card
    public int? Player1ComparedCardValue { get; private set; }
    public CardType? Player2ComparedCardType { get; private set; } // TargetPlayer's card
    public int? Player2ComparedCardValue { get; private set; }
    public Guid? BaronLoserPlayerId { get; private set; }

    // For Prince Discard
    public CardType? DiscardedByPrinceCardType { get; private set; }
    public int? DiscardedByPrinceCardValue { get; private set; }

    // For Fizzled Effects (played card is in PlayedCardType)
    public string? FizzleReason { get; private set; }

    // For Round/Game End
    public Guid? WinnerPlayerId { get; private set; }
    public string? RoundEndReason { get; private set; }
    public List<GameLogPlayerRoundSummary>? RoundPlayerSummaries { get; private set; }

    // Constructor for Priest Effect (or similar targeted card reveal)
    public GameLogEntry(GameLogEventType eventType, Guid actingPlayerId, string actingPlayerName, Guid targetPlayerId, string targetPlayerName, Guid revealedCardId, CardType revealedCardType, bool isPrivate)
    {
        // Consider a check if eventType is appropriate, or make it more generic
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        ActingPlayerId = actingPlayerId;
        ActingPlayerName = actingPlayerName ?? throw new ArgumentNullException(nameof(actingPlayerName));
        TargetPlayerId = targetPlayerId;
        TargetPlayerName = targetPlayerName ?? throw new ArgumentNullException(nameof(targetPlayerName));
        RevealedCardId = revealedCardId;
        RevealedCardType = revealedCardType ?? throw new ArgumentNullException(nameof(revealedCardType));
        IsPrivate = isPrivate;
        Message = null;
    }
    
    public GameLogEntry(GameLogEventType eventType, Guid actingPlayerId, string actingPlayerName, Guid targetPlayerId, string targetPlayerName, string message, bool isPrivate)
    {
        // Consider a check if eventType is appropriate, or make it more generic
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        ActingPlayerId = actingPlayerId;
        ActingPlayerName = actingPlayerName ?? throw new ArgumentNullException(nameof(actingPlayerName));
        TargetPlayerId = targetPlayerId;
        TargetPlayerName = targetPlayerName ?? throw new ArgumentNullException(nameof(targetPlayerName));
        IsPrivate = isPrivate;
        Message = message ?? throw new ArgumentNullException(nameof(message));;
    }

    // Constructor for general events (e.g., Handmaid, status changes, simple messages)
    public GameLogEntry(GameLogEventType eventType, Guid actingPlayerId, string actingPlayerName, string message, bool isPrivate)
    {
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        ActingPlayerId = actingPlayerId;
        ActingPlayerName = actingPlayerName ?? throw new ArgumentNullException(nameof(actingPlayerName));
        TargetPlayerId = null;
        TargetPlayerName = null;
        RevealedCardId = null;
        RevealedCardType = null;
        IsPrivate = isPrivate;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    // Constructor for simple, public, non-player-specific events (e.g., Round End with no winner)
    public GameLogEntry(GameLogEventType eventType, string message)
    {
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        ActingPlayerId = Guid.Empty; // Or a specific system/game ID
        ActingPlayerName = "Game"; // Or system name
        IsPrivate = false;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    // New Comprehensive Constructor (Example - may need refinement and other variants)
    public GameLogEntry(
        GameLogEventType eventType,
        Guid actingPlayerId,
        string actingPlayerName,
        bool isPrivate,
        string? message = null, // Message is now optional for this constructor
        Guid? targetPlayerId = null,
        string? targetPlayerName = null,
        CardType? playedCardType = null,
        CardType? guessedCardType = null,
        bool? wasGuessCorrect = null,
        CardType? player1ComparedCardType = null, // Baron
        CardType? player2ComparedCardType = null, // Baron
        Guid? baronLoserPlayerId = null,       // Baron
        CardType? discardedByPrinceCardType = null,
        Guid? revealedCardId = null,
        CardType? revealedByPriestCardType = null, // Specific for Priest if different from general RevealedCardType
        string? fizzleReason = null,
        Guid? winnerPlayerId = null,
        string? roundEndReason = null,
        List<GameLogPlayerRoundSummary>? roundPlayerSummaries = null
    )
    {
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        ActingPlayerId = actingPlayerId;
        ActingPlayerName = actingPlayerName ?? throw new ArgumentNullException(nameof(actingPlayerName));
        IsPrivate = isPrivate;
        Message = message; // Can still be set for fallback or simple text

        TargetPlayerId = targetPlayerId;
        TargetPlayerName = targetPlayerName;

        PlayedCardType = playedCardType;
        PlayedCardValue = playedCardType?.Value; // Assuming CardType has a .Value property or similar

        GuessedCardType = guessedCardType;
        GuessedCardValue = guessedCardType?.Value;
        WasGuessCorrect = wasGuessCorrect;

        Player1ComparedCardType = player1ComparedCardType;
        Player1ComparedCardValue = player1ComparedCardType?.Value;
        Player2ComparedCardType = player2ComparedCardType;
        Player2ComparedCardValue = player2ComparedCardType?.Value;
        BaronLoserPlayerId = baronLoserPlayerId;

        DiscardedByPrinceCardType = discardedByPrinceCardType;
        DiscardedByPrinceCardValue = discardedByPrinceCardType?.Value;

        RevealedCardId = revealedCardId; 
        // If revealedByPriestCardType is used, set RevealedCardType (the general property)
        RevealedCardType = revealedByPriestCardType ?? RevealedCardType; 
        // Ensure RevealedCardValue is also set if RevealedCardType is set through this param
        // For now, RevealedCardId is not in this constructor, add if needed for Priest through this path.

        FizzleReason = fizzleReason;
        WinnerPlayerId = winnerPlayerId;
        RoundEndReason = roundEndReason;
        RoundPlayerSummaries = roundPlayerSummaries;
    }

    // Private constructor for EF Core or other ORMs if needed
    private GameLogEntry() { 
        ActingPlayerName = string.Empty; 
        // Initialize other string properties to empty if they are non-nullable and not set in all constructors
        // For nullable strings, null is fine.
        RoundPlayerSummaries = new List<GameLogPlayerRoundSummary>(); // Initialize list for EF
    }
}
