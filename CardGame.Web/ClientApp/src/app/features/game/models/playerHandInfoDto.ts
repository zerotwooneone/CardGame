import { CardDto } from './cardDto';

/**
 * Represents information about a player within the PlayerGameStateDto.
 */
export interface PlayerHandInfoDto {
  playerId: string; // Changed from Guid
  name: string;
  status: number; // PlayerStatus enum value (e.g., 1=Active, 2=Eliminated)
  handCardCount: number; // Show count for all players
  playedCards?: CardDto[];
  tokensWon: number;
  isProtected: boolean;
}
