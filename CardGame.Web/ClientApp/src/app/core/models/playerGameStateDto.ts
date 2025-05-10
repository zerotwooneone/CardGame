import {PlayerHandInfoDto} from "./playerHandInfoDto";
import {CardDto} from "./cardDto";
import { GameLogEntryDto } from './gameLogEntryDto';

/**
 * Represents the game state from the perspective of a specific player,
 * including their private hand information.
 */
export interface PlayerGameStateDto {
  gameId: string; // Changed from Guid
  roundNumber: number;
  gamePhase: string;
  currentTurnPlayerId: string; // Changed from Guid
  tokensNeededToWin: number;
  players: PlayerHandInfoDto[];
  deckCardsRemaining: number;
  discardPile: CardDto[];
  publiclySetAsideCards: CardDto[]; // Added here as well
  playerHand: CardDto[]; // Actual hand cards for the requesting player
  gameLog: GameLogEntryDto[];
}
