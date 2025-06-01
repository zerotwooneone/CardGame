import { CardDto } from './cardDto';

/**
 * Represents the data payload for a Priest card reveal event.
 */
export interface PriestRevealData {
  targetPlayerId: string;
  targetPlayerName: string;
  revealedCard: CardDto;
}
