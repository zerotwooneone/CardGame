namespace CardGame.Application.DTOs;

/// <summary>
/// Represents the publicly visible state of a game for spectators.
/// </summary>
public class SpectatorGameStateDto
{
    public Guid GameId { get; set; }
    public int RoundNumber { get; set; }
    public int GamePhase { get; set; } 
    public Guid CurrentTurnPlayerId { get; set; }
    public Guid DeckDefinitionId { get; set; } // Added DeckDefinitionId
    public int TokensNeededToWin { get; set; }
    public List<SpectatorPlayerDto> Players { get; set; } = new List<SpectatorPlayerDto>();
    public int DeckCardsRemaining { get; set; } // Count only
    public List<GameLogEntryDto> GameLog { get; set; } = new List<GameLogEntryDto>(); // New: Game Logs
    // Note: SetAsideCard is omitted as its identity is secret.
    // Note: Player hands are omitted, only counts are shown via SpectatorPlayerDto.
}