export enum GameLogEntryType {
  Generic = 'Generic', // Fallback or general message
  CardPlayed = 'CardPlayed', // Player plays a card
  PriestReveal = 'PriestReveal',
  GuardGuess = 'GuardGuess',
  BaronComparison = 'BaronComparison',
  HandmaidProtection = 'HandmaidProtection',
  PrinceDiscard = 'PrinceDiscard',
  KingSwap = 'KingSwap',
  PlayerEliminated = 'PlayerEliminated',
  CardEffectFizzled = 'CardEffectFizzled',
  RoundEnded = 'RoundEnded',
  GameEnded = 'GameEnded',
  TurnStarted = 'TurnStarted',
  PlayerDrewCard = 'PlayerDrewCard'
  // Add more as needed
}
