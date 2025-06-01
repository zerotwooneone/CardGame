import { CardDto } from './cardDto';

export interface RoundEndPlayerSummaryDto {
  playerId: string;
  playerName: string;
  cardsHeld: CardDto[];
  tokensWon: number;
}
