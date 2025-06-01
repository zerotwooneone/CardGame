import {RoundEndPlayerSummaryDto} from './roundEndPlayerSummaryDto';

/**
 * Represents the overall summary sent to clients when a round ends,
 * as received by the Angular client.
 */
export interface RoundEndSummaryDto {
  reason: string; // Why the round ended
  winnerPlayerId?: string | null; // ID of the round winner (null if draw)
  playerSummaries: RoundEndPlayerSummaryDto[]; // Details for each player
}
