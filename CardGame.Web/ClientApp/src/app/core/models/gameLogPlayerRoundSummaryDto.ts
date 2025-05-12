import { CardDto } from './cardDto';

/**
 * Represents the summary for a single player within a game log entry, typically for round end events.
 */
export interface GameLogPlayerRoundSummaryDto {
  playerId: string;
  playerName: string;
  cardsHeld: CardDto[];
  tokensWon: number;
  wasActive: boolean;
}
