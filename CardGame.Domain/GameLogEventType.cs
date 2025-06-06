namespace CardGame.Domain;

public enum GameLogEventType
{
    CardPlayed = 0,       
    PriestEffect = 1,     
    HandmaidProtection = 2, 
    EffectFizzled = 3,    
    PlayerEliminated = 5, 
    BaronCompare = 6,     
    PrinceDiscard = 7,    
    KingTrade = 9,        
    CountessDiscard = 10, 
    RoundEnd = 11,        
    GameEnd = 12,         
    TokenAwarded = 13,    
    GuardHit = 14,        
    GuardMiss = 15,       
    RoundStart = 17,
    TurnStart = 18,
    PlayerDrewCard = 19,
    PrincePlayerDrawsNewCard = 20, // New event type for when Prince target draws a card
    CardSetAsidePublicly = 21   // Card set aside face up at round start (e.g., in 2-player games)
}
