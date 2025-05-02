/**
 * Represents the publicly visible state of a player for spectators.
 */
export interface SpectatorPlayerDto {
  playerId: string; // Changed from Guid
  name: string;
  status: string; // e.g., "Active", "Eliminated"
  handCardCount: number; // Count only
  playedCardTypes: number[]; // Show types of cards played this round - Consider sending type values (int[])?
  tokensWon: number;
  isProtected: boolean;
}
