import {inject, Injectable} from '@angular/core';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {catchError, Observable, throwError} from 'rxjs';
import {PlayCardRequestDto} from '../../../core/models/playCardRequestDto';
import { PlayerGameStateDto } from '../../../core/models/playerGameStateDto';
import {CreateGameRequestDto} from '../../../core/models/createGameRequestDto';
import { SpectatorGameStateDto } from '../../../core/models/spectatorGameStateDto';

@Injectable({
  // Provided locally within the game feature routing or component if lazy loaded,
  // or providedIn: 'root' if used more widely.
  providedIn: 'any' // Allows providing it at the feature level
})
export class GameActionService {
  // Base API URL for game actions
  // Use relative path - configure base URL via proxy or interceptor
  private apiUrl = '/api/Game';

  private http = inject(HttpClient);

  constructor() { }

  /**
   * Sends a request to the backend API to play a card.
   * @param gameId The ID of the game where the card is being played.
   * @param cardId The appearance ID of the card being played.
   * @param targetPlayerId The ID of the player being targeted (optional).
   * @param guessedCardValue The guessed card value (optional).
   * @returns An observable that completes on success or errors out.
   */
  playCard(gameId: string, cardId: string, targetPlayerId?: string | null, guessedCardValue?: number | null): Observable<void> {
    const payload: PlayCardRequestDto = {
      cardId: cardId,
      targetPlayerId: targetPlayerId ?? undefined,
      guessedCardType: guessedCardValue ?? undefined
    };
    const url = `${this.apiUrl}/${gameId}/play`;
    return this.http.post<void>(url, payload)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Fetches the game state from a specific player's perspective.
   * @param gameId The ID of the game.
   * @param playerId The ID of the player whose state is requested.
   * @returns An observable containing the player-specific game state.
   */
  getPlayerState(gameId: string, playerId: string): Observable<PlayerGameStateDto> {
    const url = `${this.apiUrl}/${gameId}/players/${playerId}`;
    return this.http.get<PlayerGameStateDto>(url)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Fetches the game state from a spectator's perspective.
   * @param gameId The ID of the game.
   * @returns An observable containing the spectator-specific game state.
   */
  getSpectatorGameState(gameId: string): Observable<SpectatorGameStateDto> {
    const url = `${this.apiUrl}/${gameId}`;
    return this.http.get<SpectatorGameStateDto>(url)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Sends a request to the backend API to create a new game.
   * @param payload The details for the new game (Player IDs, optional TokensToWin).
   * @returns An observable containing the ID (string) of the newly created game.
   */
  createGame(payload: CreateGameRequestDto): Observable<string> {
    const url = `${this.apiUrl}`; // POST to /api/Game
    // Backend returns the Guid string of the new game
    return this.http.post<string>(url, payload)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Basic error handler for HTTP requests.
   * Consider moving to a shared utility or interceptor.
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred performing the game action!';
    if (error.error instanceof ErrorEvent) {
      // Client-side errors
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side errors
      const errorBody = error.error; // Could be ProblemDetails, string, etc.
      if (errorBody && typeof errorBody.detail === 'string') {
        errorMessage = errorBody.detail; // Use ProblemDetails detail
      } else if (errorBody && typeof errorBody.title === 'string') {
        errorMessage = errorBody.title; // Use ProblemDetails title
      } else if (typeof errorBody === 'string' && errorBody.length > 0) {
        errorMessage = errorBody; // Use plain string error
      } else {
        errorMessage = `Server returned code ${error.status}, error message is: ${error.message}`;
      }
    }
    console.error(`GameActionService Error: ${errorMessage}`, error);
    // Return an observable that emits the error
    return throwError(() => new Error(errorMessage));
  }
}
