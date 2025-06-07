import {CardRank} from './cardRank';

/**
 * Represents basic public information about a card (e.g., for the discard pile or hand).
 */
export interface CardDto {
  appearanceId: string;
  rank: CardRank;
}
