using CardGame.Domain; // For GameLogEventType
using CardGame.Domain.Types; // For CardType

namespace CardGame.Application.DTOs;

public class GameLogEntryDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public GameLogEventType EventType { get; set; }
    public string EventTypeName => EventType.ToString(); // For easier display on client
    public Guid? ActingPlayerId { get; set; }
    public string ActingPlayerName { get; set; } = string.Empty;
    public Guid? TargetPlayerId { get; set; }
    public string? TargetPlayerName { get; set; }
    public Guid? RevealedCardId { get; set; }
    public CardType? RevealedCardType { get; set; }
    public string? RevealedCardName => RevealedCardType?.ToString(); // For easier display
    public int? RevealedCardValue => RevealedCardType?.Value;
    public bool IsPrivate { get; set; } // Client might need this if it receives mixed logs
    public string? Message { get; set; }

    // --- New Structured Properties --- 
    public CardType? PlayedCardType { get; set; }
    public string? PlayedCardName => PlayedCardType?.ToString();
    public int? PlayedCardValue => PlayedCardType?.Value;

    // For Guard Guess
    public CardType? GuessedCardType { get; set; }
    public string? GuessedCardName => GuessedCardType?.ToString();
    public int? GuessedCardValue => GuessedCardType?.Value;
    public bool? WasGuessCorrect { get; set; }

    // For Baron Comparison
    public CardType? Player1ComparedCardType { get; set; } // ActingPlayer's card
    public string? Player1ComparedCardName => Player1ComparedCardType?.ToString();
    public int? Player1ComparedCardValue => Player1ComparedCardType?.Value;
    public CardType? Player2ComparedCardType { get; set; } // TargetPlayer's card
    public string? Player2ComparedCardName => Player2ComparedCardType?.ToString();
    public int? Player2ComparedCardValue => Player2ComparedCardType?.Value;
    public Guid? BaronLoserPlayerId { get; set; }

    // For Prince Discard
    public CardType? DiscardedByPrinceCardType { get; set; }
    public string? DiscardedByPrinceCardName => DiscardedByPrinceCardType?.ToString();
    public int? DiscardedByPrinceCardValue => DiscardedByPrinceCardType?.Value;

    // For Fizzled Effects
    public string? FizzleReason { get; set; }
    // FizzledCardType would be the same as PlayedCardType for the DTO context

    // For Round/Game End
    public Guid? WinnerPlayerId { get; set; }
    public string? RoundEndReason { get; set; }
}
