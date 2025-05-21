import { GameLogEventType } from './gameLogEventType';
import { RoundEndPlayerSummaryDto } from './roundEndPlayerSummaryDto';
import { CardDto } from './cardDto';
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
  isPrivate: boolean;
  message?: string;

  // --- Properties aligned with C# GameLogEntryDto ---
  playedCard?: CardDto;
  drawnCard?: CardDto;
  discardedCard?: CardDto;

  // Priest Effect
  revealedPlayerCard?: CardDto;

  // Guard Effect
  guessedPlayerActualCard?: CardDto;
  guessedRank?: CardType;
  wasGuessCorrect?: boolean;

  // Baron Effect
  actingPlayerBaronCard?: CardDto;
  targetPlayerBaronCard?: CardDto;
  baronLoserPlayerId?: string;

  // Prince Effect
  targetDiscardedCard?: CardDto;
  targetNewCardAfterPrince?: CardDto;

  // King Effect
  revealedTradedCard?: CardDto;

  // Elimination
  revealedCardOnElimination?: CardDto;

  // Fizzled Effects
  fizzleReason?: string;

  // Round/Game End
  winnerPlayerId?: string;
  winnerPlayerName?: string;
  roundEndReason?: string;
  roundPlayerSummaries?: RoundEndPlayerSummaryDto[];
  tokensHeld?: number;
}
