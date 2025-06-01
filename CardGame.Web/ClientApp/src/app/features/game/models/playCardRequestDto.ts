/**
 * Represents the data sent to the backend when a player plays a card.
 * Matches the backend PlayCardRequestDto.
 */
export interface PlayCardRequestDto {
  /**
   * The unique ID of the specific card instance being played from the player's hand.
   */
  cardId: string; // Guid as string

  /**
   * Optional: The ID of the player being targeted by the card effect (if applicable).
   */
  targetPlayerId?: string | null; // Guid as string | null

  /**
   * Optional: The type of card being guessed (used only when playing a Guard).
   * Represented as a string (e.g., "Priest", "Baron").
   * Consider sending the integer value if CardType uses values on backend.
   */
  guessedCardType?: number | null;
}
