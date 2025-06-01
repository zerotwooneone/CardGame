namespace CardGame.Application.DTOs;

/// <summary>
/// Represents the game state from the perspective of a specific player,
/// including their private hand information.
/// </summary>
public class PlayerGameStateDto
{
    public Guid GameId { get; set; }
    public int RoundNumber { get; set; }
    public int GamePhase { get; set; }
    public Guid CurrentTurnPlayerId { get; set; }
    public int TokensNeededToWin { get; set; }
    public List<PlayerHandInfoDto> Players { get; set; } = new List<PlayerHandInfoDto>();
    public int DeckCardsRemaining { get; set; }
    public List<CardDto> DiscardPile { get; set; } = new List<CardDto>();

    /// <summary>
    /// Gets or sets the game log entries.
    /// </summary>
    public List<GameLogEntryDto> GameLog { get; set; } = new List<GameLogEntryDto>();

    /// <summary>
    /// Gets or sets the actual cards in the requesting player's hand.
    /// This is ONLY populated for the player making the request.
    /// </summary>
    public List<CardDto> PlayerHand { get; set; } = new List<CardDto>();
}