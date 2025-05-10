import {CardDto} from './cardDto';
import {GameLogEntryType} from './gameLogEntryType';

export interface GameLogEntry {
  id: string; // Unique ID for the log entry (e.g., timestamp + random suffix)
  type: GameLogEntryType;
  timestamp: Date;
  isPrivate: boolean; // True if this log entry is only for the current player

  actorPlayerId?: string;
  actorPlayerName?: string; // For display

  targetPlayerId?: string;
  targetPlayerName?: string; // For display

  cardPlayed?: CardDto; // The card that was played by the actor
  cardPlayedType?: number; // Numeric type of card played

  // Specific data for different event types
  revealedCard?: CardDto; // For PriestReveal
  guessedCardTypeValue?: number; // For GuardGuess (the numeric value of the guessed type)
  guessWasCorrect?: boolean; // For GuardGuess

  comparedPlayerId?: string; // For Baron
  comparedPlayerName?: string; // For Baron
  comparedPlayerCardType?: number; // For Baron
  actorCardType?: number; // For Baron (actor's card)
  baronLoserId?: string | null; // For Baron

  discardedCard?: CardDto; // For PrinceDiscard

  swappedWithPlayerId?: string; // For KingSwap
  swappedWithPlayerName?: string; // For KingSwap

  eliminatedPlayerId?: string; // For PlayerEliminated
  eliminationReason?: string; // For PlayerEliminated

  fizzledCardType?: number; // For CardEffectFizzled
  fizzleReason?: string; // For CardEffectFizzled

  roundWinnerId?: string | null; // For RoundEnded
  roundEndReason?: string; // For RoundEnded

  gameWinnerId?: string; // For GameEnded

  message?: string; // For generic messages or additional text
}
