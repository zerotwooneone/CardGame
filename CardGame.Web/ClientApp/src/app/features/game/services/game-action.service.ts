import {inject, Injectable} from '@angular/core';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {catchError, Observable, throwError} from 'rxjs';
import {PlayCardRequestDto} from '../../../core/models/playCardRequestDto';

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
   * @param payload The details of the card play action (CardId, TargetPlayerId, GuessedCardType).
   * @returns An observable that completes on success or errors out.
   */
  playCard(gameId: string, payload: PlayCardRequestDto): Observable<void> {
    const url = `${this.apiUrl}/${gameId}/play`;
    // Use post<void> since the backend likely returns 200 OK with no body on success
    return this.http.post<void>(url, payload)
      .pipe(
        catchError(this.handleError)
      );
  }

  // --- Add other game action methods here if needed ---
  // e.g., startGame(), forfeitGame(), etc.

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
