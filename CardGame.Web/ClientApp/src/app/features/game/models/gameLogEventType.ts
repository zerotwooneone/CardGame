export enum GameLogEventType {
  CardPlayed = 0,
  PriestEffect = 1,
  HandmaidProtection = 2,
  EffectFizzled = 3,
  // GuardGuess = 4, // Assuming this is effectively replaced by Hit/Miss
  PlayerEliminated = 5,
  BaronCompare = 6,
  PrinceDiscard = 7,
  // PrinceEffectFailed = 8, // May be covered by EffectFizzled or specific PrinceDiscard visuals
  KingTrade = 9,
  CountessDiscard = 10,
  RoundEnd = 11,
  GameEnd = 12,
  TokenAwarded = 13,
  GuardHit = 14,
  GuardMiss = 15,
  PlayerDrewCard = 19,
  RoundStart = 17,
  TurnStart = 18,
  PrincePlayerDrawsNewCard = 20
}
