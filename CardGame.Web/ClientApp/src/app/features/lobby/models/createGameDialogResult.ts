/**
 * Data returned *from* the CreateGameDialogComponent upon successful confirmation.
 */
export interface CreateGameDialogResult {
  /** The Player IDs of the selected opponents. */
  selectedOpponentIds: string[];
  /** The ID of the selected deck. */
  deckId: string;
  /** Optional: Any newly entered and validated friend codes (as JSON strings) */
  newlyValidatedFriendCodes?: string[];
}
