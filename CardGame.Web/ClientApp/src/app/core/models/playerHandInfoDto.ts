/**
 * Represents information about a player within the PlayerGameStateDto.
 */
export interface PlayerHandInfoDto {
  playerId: string; // Changed from Guid
  name: string;
  status: number;
  handCardCount: number; // Show count for all players
  playedCardTypes?: number[]; // Consider sending type values (int[])?
  tokensWon: number;
  isProtected: boolean;
}
