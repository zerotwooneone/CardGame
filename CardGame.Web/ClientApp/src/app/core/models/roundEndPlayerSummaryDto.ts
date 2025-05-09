import {CardDto} from './cardDto';

/**
 * Represents the summary details for a single player at the end of a round,
 * as received by the Angular client.
 */
export interface RoundEndPlayerSummaryDto {
  playerId: string;
  playerName: string;
  finalHeldCard?: CardDto | null; // The card held at the end (null if eliminated/empty)
  discardPileValues: number[]; // Values (ranks) of cards discarded this round
  tokensWon: number; // Total tokens after the round
}
