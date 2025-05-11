import { GameLogEventType } from './gameLogEventType';
import {CardType} from './cardType';

export interface GameLogEntryDto {
  id: string;
  timestamp: Date;
  eventType: GameLogEventType;
  eventTypeName: string;
  actingPlayerId?: string;
  actingPlayerName: string;
  targetPlayerId?: string;
  targetPlayerName?: string;
  revealedCardId?: string;
  revealedCardValue?: CardType;
  isPrivate: boolean;
  message?: string;

  // Structured properties
  playedCardValue?: CardType;

  guessedCardValue?: CardType;
  wasGuessCorrect?: boolean;

  player1ComparedCardValue?: CardType;
  player2ComparedCardValue?: CardType;
  baronLoserPlayerId?: string;

  discardedByPrinceCardValue?: CardType;

  cardResponsibleForEliminationValue?: CardType;

  fizzleReason?: string;

  winnerPlayerId?: string;
  roundEndReason?: string;
  tokensHeld?: number;
  cardDrawnValue?: CardType;
}
