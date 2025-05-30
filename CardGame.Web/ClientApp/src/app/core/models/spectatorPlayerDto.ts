import { CardDto } from './cardDto';

/**
 * Represents the publicly visible state of a player for spectators.
 */
export interface SpectatorPlayerDto {
  playerId: string; // Changed from Guid
  name: string;
  status: number; // (e.g., 1=Active, 2=Eliminated)
  handCardCount: number; // Count only
  playedCards: CardDto[];
  tokensWon: number;
  isProtected: boolean;
}
