using CardGame.Domain.Types; // For CardType
using CardGame.Domain.Game; // For Card

namespace CardGame.Domain;

public class GameLogEntry
{
    public Guid Id { get; private set; } 
    public DateTimeOffset Timestamp { get; private set; }
    public GameLogEventType EventType { get; set; } 
    public Guid? ActingPlayerId { get; set; }
    public string ActingPlayerName { get; set; }
    public Guid? TargetPlayerId { get; set; }
    public string? TargetPlayerName { get; set; }
    public string? RevealedCardAppearanceId { get; set; } 
    public CardType? RevealedCardType { get; set; } 
    public bool IsPrivate { get; set; }
    public string? Message { get; set; }

    // --- New Structured Properties for Visual Story --- 
    public string? PlayedCardAppearanceId { get; set; } 
    public CardType? PlayedCardType { get; set; }

    // For Guard Guess
    public string? GuessedCardAppearanceId { get; set; } 
    public CardType? GuessedCardType { get; set; } 
    public bool? WasGuessCorrect { get; set; }   

    // For Baron Comparison
    public string? Player1ComparedCardAppearanceId { get; set; } 
    public CardType? Player1ComparedCardType { get; set; } 
    public string? Player2ComparedCardAppearanceId { get; set; } 
    public CardType? Player2ComparedCardType { get; set; } 
    public Guid? BaronLoserPlayerId { get; set; }        

    // For Prince Discard
    public string? DiscardedByPrinceCardAppearanceId { get; set; } 
    public CardType? DiscardedByPrinceCardType { get; set; } 

    // For Eliminations
    public string? CardResponsibleForEliminationAppearanceId { get; set; } 
    public CardType? CardResponsibleForElimination { get; set; } 

    // For Fizzled Effects (played card is in PlayedCardType)
    public string? FizzleReason { get; set; } 

    // For Round/Game End
    public Guid? WinnerPlayerId { get; set; } 
    public string? RoundEndReason { get; set; } 
    public List<GameLogPlayerRoundSummary>? RoundPlayerSummaries { get; set; }
    public int? TokensHeld { get; set; }
    public string? CardDrawnAppearanceId { get; set; } 
    public CardType? CardDrawnType { get; set; }

    // Base constructor - initializes essential fields. Others can be set via property initializers.
    public GameLogEntry(GameLogEventType eventType, Guid? actingPlayerId, string actingPlayerName, string? message, bool isPrivate = false)
    {
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        ActingPlayerId = actingPlayerId;
        ActingPlayerName = actingPlayerName;
        Message = message;
        IsPrivate = isPrivate;
        // Initialize other nullable properties to null by default
        TargetPlayerId = null;
        TargetPlayerName = null;
        RevealedCardAppearanceId = null;
        RevealedCardType = null;
        PlayedCardAppearanceId = null;
        PlayedCardType = null;
        GuessedCardAppearanceId = null;
        GuessedCardType = null;
        WasGuessCorrect = null;
        Player1ComparedCardAppearanceId = null;
        Player1ComparedCardType = null;
        Player2ComparedCardAppearanceId = null;
        Player2ComparedCardType = null;
        BaronLoserPlayerId = null;
        DiscardedByPrinceCardAppearanceId = null;
        DiscardedByPrinceCardType = null;
        CardResponsibleForEliminationAppearanceId = null;
        CardResponsibleForElimination = null;
        FizzleReason = null;
        WinnerPlayerId = null;
        RoundEndReason = null;
        RoundPlayerSummaries = null;
        CardDrawnAppearanceId = null;
        CardDrawnType = null;
    }

    // Constructor for events with a target player
    public GameLogEntry(GameLogEventType eventType, Guid actingPlayerId, string actingPlayerName, Guid targetPlayerId, string targetPlayerName, string? message, bool isPrivate = false)
        : this(eventType, actingPlayerId, actingPlayerName, message, isPrivate)
    {
        TargetPlayerId = targetPlayerId;
        TargetPlayerName = targetPlayerName;
    }

    // Constructor for simple, public, non-player-specific events (e.g., Round End with no winner)
    public GameLogEntry(GameLogEventType eventType, string message)
        : this(eventType, Guid.Empty, "Game", message, false) // Assuming Guid.Empty and "Game" for system events
    {
    }

    // Private parameterless constructor for EF Core or deserialization if needed
    private GameLogEntry() 
    {
        // Required for EF Core to materialize objects from DB
        // Initialize critical non-nullable properties to sensible defaults if possible
        Id = Guid.NewGuid(); // Should be overwritten by DB value
        Timestamp = DateTimeOffset.UtcNow; // Should be overwritten
        ActingPlayerName = string.Empty; // Should be overwritten
        Message = string.Empty; // Should be overwritten
    }

    // --- Nested class for player summaries in round/game end logs ---
    public class GameLogPlayerRoundSummary
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; }
        public List<Card> CardsHeld { get; set; } = new List<Card>(); 
        public int Score { get; set; } // e.g., tokens won
        public bool WasActive { get; set; } // Was player active at round end?

        public GameLogPlayerRoundSummary(Guid playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
    }
}
