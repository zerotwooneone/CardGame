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
    public string? RevealedCardAppearanceId { get; set; } 
    public int? RevealedCardValue { get; set; }
    public bool IsPrivate { get; set; }
    public string? Message { get; set; }

    // --- Structured Properties ---
    public string? PlayedCardAppearanceId { get; set; } 
    public int? PlayedCardValue { get; set; }

    // For Guard Guess
    public string? GuessedCardAppearanceId { get; set; } 
    public int? GuessedCardValue { get; set; }
    public bool? WasGuessCorrect { get; set; }

    // For Baron Comparison
    public string? Player1ComparedCardAppearanceId { get; set; } 
    public int? Player1ComparedCardValue { get; set; }
    public string? Player2ComparedCardAppearanceId { get; set; } 
    public int? Player2ComparedCardValue { get; set; }
    public Guid? BaronLoserPlayerId { get; set; }

    // For Prince Discard
    public string? DiscardedByPrinceCardAppearanceId { get; set; } 
    public int? DiscardedByPrinceCardValue { get; set; }

    // For Eliminations
    public string? CardResponsibleForEliminationAppearanceId { get; set; } 
    public int? CardResponsibleForEliminationValue { get; set; }

    // For Fizzled Effects
    public string? FizzleReason { get; set; }

    // For Round/Game End
    public Guid? WinnerPlayerId { get; set; }
    public string? RoundEndReason { get; set; }
    public List<RoundEndPlayerSummaryDto>? RoundPlayerSummaries { get; set; }
    public int? TokensHeld { get; set; }
    public string? CardDrawnAppearanceId { get; set; } 
    public int? CardDrawnValue { get; set; }
}
