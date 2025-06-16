namespace CardGame.Domain;

/// <summary>
/// intended for game-specific events that are relevant to players. and the observable flow of the game. Avoid adding general application/system-level events. or internal error/diagnostic types here. Such events should be handled by ILogger. 
/// </summary>
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
    //GameEnd = 12, //we do not log game end       
    TokenAwarded = 13,    
    GuardHit = 14,        
    GuardMiss = 15,       
    RoundStart = 17,
    TurnStart = 18,
    PlayerDrewCard = 19,
    //PrincePlayerDrawsNewCard = 20, // we do not log this
    CardSetAsidePublicly = 21,   // Card set aside face up at round start (e.g., in 2-player games)

    // --- Premium Expansion Events ---
    AssassinRevealed = 22,              // A player revealed the Assassin in response to a Guard
    JesterTokenAssigned = 23,           // The Jester token was given to a player
    CardinalEffect = 24,                // The Cardinal was used to force a hand trade
    BaronessEffect = 25,                // The Baroness was used to view one or more hands
    SycophantTargetSet = 26,            // The Sycophant mandated a target for the next effect
    DowagerQueenEffect = 27,            // The Dowager Queen was used to compare hands
    BishopGuessMade = 28,               // The Bishop was used to guess a card in a hand
    //BishopTargetDiscardedAndRedrew = 29, // we do not log this

    // General game events (use with specific context in GameLogEntry.Message)
    //CardDiscarded = 30, // we do not log this
    DeckEmpty = 31,                  // The deck (and set-aside) is empty, cannot draw
    
    DowagerQueenCompare = 32,
    BishopGuessCorrect = 33,
    BishopGuessIncorrect = 34,
    AssassinMarked = 35,
    CountEffect = 36,
    SycophantEffect = 37
}
