/**
 * Data returned *from* the CreateGameDialogComponent upon successful confirmation.
 */
export interface CreateGameDialogResult {
  /** The Player IDs of the selected opponents. */
  selectedOpponentIds: string[];
  /** Optional: Any newly entered and validated friend codes (as JSON strings) */
  newlyValidatedFriendCodes?: string[];
}
