namespace CardGame.Domain;

public enum GameLogEventType
{
    CardPlayed = 0,       // New: Generic card play
    PriestEffect = 1,
    HandmaidProtection = 2,
    EffectFizzled = 3,
    GuardGuess = 4,
    PlayerEliminated = 5, // Existing: PlayerKnockedOut, matches PlayerEliminated event
    BaronCompare = 6,
    PrinceDiscard = 7,
    PrinceEffectFailed = 8,
    KingTrade = 9,          // This might be covered by CardPlayed if King effect has no separate log detail
    CountessDiscard = 10,   // This is likely covered by CardPlayed (Countess played)
    RoundEnd = 11,
    GameEnd = 12,
    TokenAwarded = 13     // New: Player awarded a token
}
