import {SpectatorPlayerDto} from "./spectatorPlayerDto";
import {CardDto} from "./cardDto";

/**
 * Represents the publicly visible state of a game for spectators.
 */
export interface SpectatorGameStateDto {
  gameId: string; // Changed from Guid
  roundNumber: number;
  gamePhase: string; // e.g., "RoundInProgress", "GameOver"
  currentTurnPlayerId: string; // Changed from Guid
  tokensNeededToWin: number;
  players: SpectatorPlayerDto[];
  deckCardsRemaining: number; // Count only
  discardPile: CardDto[];
}
