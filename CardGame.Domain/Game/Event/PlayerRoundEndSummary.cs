namespace CardGame.Domain.Game.Event;

public record PlayerRoundEndSummary(
    Guid PlayerId,
    string PlayerName, 
    List<Card> CardsHeld, 
    bool IsWinner,        
    int TokensAwarded     
);