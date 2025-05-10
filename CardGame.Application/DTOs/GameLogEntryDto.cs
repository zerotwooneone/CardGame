using CardGame.Domain; // For GameLogEventType
using CardGame.Domain.Types; // For CardType

namespace CardGame.Application.DTOs;

public class GameLogEntryDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public GameLogEventType EventType { get; set; }
    public string EventTypeName => EventType.ToString(); // For easier display on client
    public Guid ActingPlayerId { get; set; }
    public string ActingPlayerName { get; set; } = string.Empty;
    public Guid? TargetPlayerId { get; set; }
    public string? TargetPlayerName { get; set; }
    public Guid? RevealedCardId { get; set; }
    public CardType? RevealedCardType { get; set; }
    public string? RevealedCardName => RevealedCardType?.ToString(); // For easier display
    public bool IsPrivate { get; set; } // Client might need this if it receives mixed logs
    public string? Message { get; set; }
}
