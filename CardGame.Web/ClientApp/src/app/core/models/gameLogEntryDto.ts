import { GameLogEventType } from './gameLogEventType';
import { CardType } from './cardType';

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
  revealedCardType?: CardType;
  isPrivate: boolean;
  message?: string;

  // Structured properties
  playedCardType?: CardType;
  playedCardValue?: number;

  guessedCardType?: CardType;
  guessedCardValue?: number;
  wasGuessCorrect?: boolean;

  player1ComparedCardType?: CardType;
  player1ComparedCardValue?: number;
  player2ComparedCardType?: CardType;
  player2ComparedCardValue?: number;
  baronLoserPlayerId?: string;

  discardedByPrinceCardType?: CardType;
  discardedByPrinceCardValue?: number;

  cardResponsibleForElimination?: CardType;

  fizzleReason?: string;

  winnerPlayerId?: string;
  roundEndReason?: string;
  roundPlayerSummaries?: GameLogPlayerRoundSummaryDto[];
  tokensHeld?: number;
  cardDrawnType?: CardType;
}

export interface GameLogPlayerRoundSummaryDto {
  playerId: string;
  playerName: string;
  cardsHeld: CardType[];
  score: number;
  wasActive: boolean;
}
