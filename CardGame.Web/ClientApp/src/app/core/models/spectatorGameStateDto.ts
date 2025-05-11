import {SpectatorPlayerDto} from "./spectatorPlayerDto";
import {CardDto} from "./cardDto";
import { GameLogEntryDto } from './gameLogEntryDto';

/**
 * Represents the publicly visible state of a game for spectators.
 */
export interface SpectatorGameStateDto {
  gameId: string;
  roundNumber: number;
  gamePhase: string;
  currentTurnPlayerId: string;
  tokensNeededToWin: number;
  players: SpectatorPlayerDto[];
  deckCardsRemaining: number;
  discardPile: CardDto[];
  publiclySetAsideCards: CardDto[];
  gameLog: GameLogEntryDto[];
  // Note: SetAsideCard (face-down) is omitted as its identity is secret.
}
