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
    public bool IsPrivate { get; set; }
    public string? Message { get; set; }

    // --- New/Updated Structured Properties based on Domain GameLogEntry ---
    public CardDto? PlayedCard { get; set; }                // Card played by the acting player
    public CardDto? DrawnCard { get; set; }                 // Card drawn by a player
    public CardDto? DiscardedCard { get; set; }             // Card discarded (e.g., Countess, or a general discard not tied to Prince's effect on a target)
    
    // Priest Effect
    public CardDto? RevealedPlayerCard { get; set; }        // Card revealed by Priest's effect

    // Guard Effect
    public CardDto? GuessedPlayerActualCard { get; set; }   // Actual card of the player targeted by Guard (if guess was correct or card revealed)
    public int? GuessedRank { get; set; }                   // The rank guessed by Guard (e.g., 5 for King)
    public bool? WasGuessCorrect { get; set; }             // Was the Guard's guess correct

    // Baron Effect
    public CardDto? ActingPlayerBaronCard { get; set; }     // Acting player's card in Baron comparison
    public CardDto? TargetPlayerBaronCard { get; set; }      // Target player's card in Baron comparison
    public Guid? BaronLoserPlayerId { get; set; }           // ID of the player who lost the Baron comparison

    // Prince Effect
    public CardDto? TargetDiscardedCard { get; set; }       // Card discarded by the target of Prince
    public CardDto? TargetNewCardAfterPrince { get; set; }  // New card drawn by the target of Prince (if they didn't draw Princess)

    // King Effect
    public CardDto? RevealedTradedCard { get; set; }        // Card revealed/received by the acting player after King's effect (target's original card)
                                                            // Note: The other card involved in the trade (acting player's original card) is `PlayedCard`

    // Elimination
    public CardDto? RevealedCardOnElimination { get; set; } // Card revealed by a player when they are eliminated (e.g. Princess)
    
    // Fizzled Effects (already existed, seems okay)
    public string? FizzleReason { get; set; }

    // Round/Game End (already existed, seem okay, but WinnerPlayerName might be an issue if it was removed from domain)
    public Guid? WinnerPlayerId { get; set; }
    public string? WinnerPlayerName {get; set;} // Added this back temporarily as it was in build errors, may need removal if not in domain
    public string? RoundEndReason { get; set; }
    public List<RoundEndPlayerSummaryDto>? RoundPlayerSummaries { get; set; } // Renamed from RoundEndPlayerSummaries
    public int? TokensHeld { get; set; } 
}
