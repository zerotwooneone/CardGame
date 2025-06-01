/**
 * Represents the current phase of the game, mirroring CardGame.Domain.Types.GamePhase.
 */
export enum GamePhase {
  NotStarted = 1,
  RoundInProgress = 2,
  RoundOver = 3, // A round has ended; awaiting next round or game end.
  GameOver = 4    // The game has concluded.
}
