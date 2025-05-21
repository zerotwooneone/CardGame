import {CardType} from './cardType';

/**
 * Represents basic public information about a card (e.g., for the discard pile or hand).
 */
export interface CardDto {
  appearanceId: string;
  rank: CardType;
}
