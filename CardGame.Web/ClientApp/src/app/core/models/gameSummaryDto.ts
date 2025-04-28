/**
 * Represents summary information about a game displayed in the lobby.
 */
export interface GameSummaryDto {
  gameId: string; // Changed from Guid
  playerCount: number;
  status: string; // e.g., "Waiting", "InProgress", "Finished"
  createdBy: string; // Username of creator
  // Optional: Add other relevant summary info, like player names list
  // playerNames?: string[];
}
