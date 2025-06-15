import {RankDto} from '@features/game/models/rankDto';

/**
 * Represents basic public information about a card (e.g., for the discard pile or hand).
 */
export interface CardDto {
  appearanceId: string;
  rank: RankDto;
}

