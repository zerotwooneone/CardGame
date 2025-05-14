import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CardDto } from '../../../core/models/cardDto';

@Injectable({
  providedIn: 'any'
})
export class DeckService {
  private apiUrl = '/api/deck'; // Base URL for deck-related endpoints
  private http = inject(HttpClient);

  // Using an arbitrary but valid GUID. The backend currently ignores this specific ID for fetching the default deck.
  private hardcodedDeckId = '00000000-0000-0000-0000-000000000001';

  constructor() { }

  /**
   * Fetches the default game deck from the backend using a hardcoded deck ID.
   * @returns An Observable emitting an array of CardDto.
   */
  getDefaultDeck(): Observable<CardDto[]> {
    // Calls GET /api/deck/{hardcodedDeckId}
    return this.http.get<CardDto[]>(`${this.apiUrl}/${this.hardcodedDeckId}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An unknown error occurred!';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      const errorBody = error.error;
      if (errorBody && errorBody.title) {
        errorMessage = `Error ${error.status}: ${errorBody.title}`;
      } else if (typeof errorBody === 'string' && errorBody.length > 0) {
        errorMessage = errorBody;
      } else {
        errorMessage = `Server returned code ${error.status}, error message is: ${error.message}`;
      }
    }
    console.error(`DeckService Error: ${errorMessage}`, error);
    return throwError(() => new Error(errorMessage));
  }
}
