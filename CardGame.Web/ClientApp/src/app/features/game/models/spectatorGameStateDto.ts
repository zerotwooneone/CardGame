import {SpectatorPlayerDto} from "./spectatorPlayerDto";
import {CardDto} from "./cardDto";
import { GameLogEntryDto } from './gameLogEntryDto';
import { GamePhase } from './gamePhase';

/**
 * Represents the publicly visible state of a game for spectators.
 */
export interface SpectatorGameStateDto {
  gameId: string;
  roundNumber: number;
  gamePhase: GamePhase;
  currentTurnPlayerId: string;
  deckDefinitionId: string;
  tokensNeededToWin: number;
  players: SpectatorPlayerDto[];
  deckCardsRemaining: number;
  discardPile: CardDto[];
  gameLog: GameLogEntryDto[];
}
