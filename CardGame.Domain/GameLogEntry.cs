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
    public CardType? RevealedCardType { get; private set; }
    public bool IsPrivate { get; private set; }
    public string? Message { get; private set; }

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

    // Private constructor for EF Core or other ORMs if needed
    private GameLogEntry() { 
        ActingPlayerName = string.Empty; 
    }
}
