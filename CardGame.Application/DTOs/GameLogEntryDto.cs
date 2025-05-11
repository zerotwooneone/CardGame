using CardGame.Domain; // For GameLogEventType
using CardGame.Domain.Types; // For CardType
using System.Collections.Generic;

namespace CardGame.Application.DTOs;

public class GameLogEntryDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public GameLogEventType EventType { get; set; }
    public string EventTypeName => EventType.ToString();
    public Guid? ActingPlayerId { get; set; }
    public string ActingPlayerName { get; set; } = string.Empty;
    public Guid? TargetPlayerId { get; set; }
    public string? TargetPlayerName { get; set; }
    public Guid? RevealedCardId { get; set; }
    public CardType? RevealedCardType { get; set; }
    public bool IsPrivate { get; set; }
    public string? Message { get; set; }

    // --- Structured Properties ---
    public CardType? PlayedCardType { get; set; }
    public int? PlayedCardValue { get; set; }

    // For Guard Guess
    public CardType? GuessedCardType { get; set; }
    public int? GuessedCardValue { get; set; }
    public bool? WasGuessCorrect { get; set; }

    // For Baron Comparison
    public CardType? Player1ComparedCardType { get; set; }
    public int? Player1ComparedCardValue { get; set; }
    public CardType? Player2ComparedCardType { get; set; }
    public int? Player2ComparedCardValue { get; set; }
    public Guid? BaronLoserPlayerId { get; set; }

    // For Prince Discard
    public CardType? DiscardedByPrinceCardType { get; set; }
    public int? DiscardedByPrinceCardValue { get; set; }

    // For Eliminations
    public CardType? CardResponsibleForElimination { get; set; }

    // For Fizzled Effects
    public string? FizzleReason { get; set; }

    // For Round/Game End
    public Guid? WinnerPlayerId { get; set; }
    public string? RoundEndReason { get; set; }
    public List<GameLogPlayerRoundSummaryDto>? RoundPlayerSummaries { get; set; }
    public int? TokensHeld { get; set; }
    public CardType? CardDrawnType { get; set; }

    // --- Nested DTO for player summaries ---
    public class GameLogPlayerRoundSummaryDto
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public List<CardType> CardsHeld { get; set; } = new List<CardType>();
        public int Score { get; set; }
        public bool WasActive { get; set; }
    }
}
