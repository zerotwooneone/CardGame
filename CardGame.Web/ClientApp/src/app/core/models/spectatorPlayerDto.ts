/**
 * Represents the publicly visible state of a player for spectators.
 */
export interface SpectatorPlayerDto {
  playerId: string; // Changed from Guid
  name: string;
  status: number; // (e.g., 1=Active, 2=Eliminated)
  handCardCount: number; // Count only
  playedCardTypes: number[]; // Show types of cards played this round - Consider sending type values (int[])?
  tokensWon: number;
  isProtected: boolean;
}
