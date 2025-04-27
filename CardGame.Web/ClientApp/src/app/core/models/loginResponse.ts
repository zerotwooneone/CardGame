/**
 * Matches the backend LoginResponseDto
 */
export interface LoginResponse {
  message?: string;
  username?: string;
  playerId?: string; // Guid is string in TypeScript/JSON
}
