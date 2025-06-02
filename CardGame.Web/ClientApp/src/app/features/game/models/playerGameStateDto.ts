import {PlayerHandInfoDto} from "./playerHandInfoDto";
import {CardDto} from "./cardDto";
import { GameLogEntryDto } from './gameLogEntryDto';
import {GamePhase} from './gamePhase';

/**
 * Represents the game state from the perspective of a specific player,
 * including their private hand information.
 */
export interface PlayerGameStateDto {
  gameId: string; // Changed from Guid
  roundNumber: number;
  gamePhase: GamePhase;
  currentTurnPlayerId: string;
  deckDefinitionId: string;
  tokensNeededToWin: number;
  players: PlayerHandInfoDto[];
  deckCardsRemaining: number;
  discardPile: CardDto[];
  playerHand: CardDto[];
  gameLog: GameLogEntryDto[];
}
