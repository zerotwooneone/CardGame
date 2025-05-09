﻿/**
 * Represents basic public information about a card (e.g., for the discard pile or hand).
 */
export interface CardDto {
  id: string; // Changed from Guid to string for frontend
  type: number; // e.g., "Guard", "Princess" - Consider sending type value (int) instead?
  // AppearanceId removed
}
