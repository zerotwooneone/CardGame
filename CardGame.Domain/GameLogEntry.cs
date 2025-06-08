using CardGame.Domain.Types; // For CardType
using CardGame.Domain.Game;
using CardRank = CardGame.Domain.BaseGame.CardRank; // For Card

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
    public bool IsPrivate { get; set; }
    public string? Message { get; set; }

    // --- New Card Properties using Card Game.Domain.Game.Card --- 
    public Card? PlayedCard { get; set; }
    public Card? DrawnCard { get; set; }
    public Card? DiscardedCard { get; set; } // e.g., for Countess forced discard
    public Card? RevealedPlayerCard { get; set; } // e.g., Priest effect
    public Card? ActingPlayerBaronCard { get; set; } // Card of the acting player in Baron compare
    public Card? TargetPlayerBaronCard { get; set; } // Card of the target player in Baron compare
    public Card? TargetDiscardedCard { get; set; } // Card discarded by the target of a Prince
    public Card? TargetNewCardAfterPrince { get; set; } // New card drawn by the target of a Prince (if applicable)
    public Card? RevealedTradedCard { get; set; } // Card revealed by the target player after a King trade
    public Card? RevealedCardOnElimination { get; set; } // E.g., the losing card in a Baron compare, or the revealed card in a Guard hit
    public Card? GuessedPlayerActualCard { get; set; } // The actual card revealed if a Guard guess hits
    
    // For Guard Guess specifics
    public CardRank? GuessedRank { get; set; } // The rank guessed by the Guard player
    public bool? WasGuessCorrect { get; set; }   

    // For Baron Comparison specifics (loser ID)
    public Guid? BaronLoserPlayerId { get; set; }        

    // For Fizzled Effects (played card is in PlayedCard)
    public string? FizzleReason { get; set; } 

    // For Round/Game End
    public Guid? WinnerPlayerId { get; set; } 
    public string? WinnerPlayerName { get; set; } // Added for consistency with DTO
    public string? RoundEndReason { get; set; } 
    public List<GameLogPlayerRoundSummary>? RoundPlayerSummaries { get; set; }
    public int? TokensHeld { get; set; } // Typically used for TokenAwarded event
    

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
        
        // Initialize new Card? properties to null
        PlayedCard = null;
        DrawnCard = null;
        DiscardedCard = null;
        RevealedPlayerCard = null;
        ActingPlayerBaronCard = null;
        TargetPlayerBaronCard = null;
        TargetDiscardedCard = null;
        TargetNewCardAfterPrince = null;
        RevealedTradedCard = null;
        RevealedCardOnElimination = null;
        GuessedPlayerActualCard = null;
        GuessedRank = null;

        // Initialize other nullable properties to null by default
        TargetPlayerId = null;
        TargetPlayerName = null;
        WasGuessCorrect = null;
        BaronLoserPlayerId = null;
        FizzleReason = null;
        WinnerPlayerId = null;
        WinnerPlayerName = null;
        RoundEndReason = null;
        RoundPlayerSummaries = null;
        TokensHeld = null;
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
        // Message = string.Empty; // Message is nullable, no need to default to empty
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
         // Parameterless constructor for deserialization/EF Core if needed
        private GameLogPlayerRoundSummary() 
        {
            PlayerName = string.Empty; // Initialize to avoid null reference if used incorrectly
        }
    }
}
