/**
 * Matches the backend LoginRequestDto
 */
export interface LoginRequest {
  username: string;
  password?: string; // Password might be optional if using other auth methods later
}
