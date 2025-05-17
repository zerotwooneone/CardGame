import { Injectable, inject, signal, computed, effect, WritableSignal, Signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { EMPTY, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { CardDto } from '../../../core/models/cardDto';

export interface DeckDefinitionDto {
  cards: CardDto[];
  backAppearanceId: string;
}

@Injectable({
  providedIn: 'any'
})
export class DeckService {
  private apiUrl = '/api/deck';
  private http = inject(HttpClient);

  // --- Writable Signals ---
  public readonly selectedDeckId: WritableSignal<string> = signal('00000000-0000-0000-0000-000000000001');
  private readonly _deckDefinition: WritableSignal<DeckDefinitionDto | null> = signal(null);
  private readonly _isLoading: WritableSignal<boolean> = signal(false);
  private readonly _error: WritableSignal<string | null> = signal(null);

  // --- Readonly Signals (Public API) ---
  public readonly deckDefinition: Signal<DeckDefinitionDto | null> = this._deckDefinition.asReadonly();
  public readonly isLoading: Signal<boolean> = this._isLoading.asReadonly();
  public readonly error: Signal<string | null> = this._error.asReadonly();

  // --- Computed Signals ---
  public readonly cards: Signal<CardDto[]> = computed(() => this._deckDefinition()?.cards ?? []);
  public readonly rankMap: Signal<Map<string, number>> = computed(() => {
    const map = new Map<string, number>();
    this.cards().forEach(card => {
      map.set(card.appearanceId, card.rank);
    });
    return map;
  });
  public readonly backAppearanceId: Signal<string | null> = computed(() => {
    return this._deckDefinition()?.backAppearanceId ?? null;
  });

  constructor() {
    // Effect to fetch deck when selectedDeckId changes or on initialization
    effect(() => {
      const deckId = this.selectedDeckId();
      if (deckId) {
        this._isLoading.set(true);
        this._deckDefinition.set(null); // Clear previous deck
        this._error.set(null); // Clear previous error

        this.http.get<DeckDefinitionDto>(`${this.apiUrl}/${deckId}`).pipe(
          tap(data => {
            this._deckDefinition.set(data);
            this._isLoading.set(false);
          }),
          catchError(err => {
            this.handleError(err); // handleError will set _error
            this._isLoading.set(false);
            this._deckDefinition.set(null); // Ensure deck remains null on error
            return EMPTY; // Return EMPTY to complete the observable chain and prevent stream death
          })
        ).subscribe(); // Subscribe to trigger the HTTP request
      } else {
        // Optional: Handle case where deckId is null or empty if that's possible
        this._deckDefinition.set(null);
        this._isLoading.set(false);
        this._error.set(null);
      }
    });
  }

  /**
   * Gets the rank of a card by its AppearanceId using the computed rankMap signal.
   * @param appearanceId The appearance ID of the card.
   * @returns The rank of the card, or undefined if not found.
   */
  public getRankByAppearanceId(appearanceId: string): number | undefined {
    return this.rankMap().get(appearanceId);
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An unknown error occurred!';
    // Check if ErrorEvent is defined (for SSR/prerendering safety)
    if (typeof ErrorEvent !== 'undefined' && error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      const errorBody = error.error;
      if (errorBody && errorBody.title) {
        errorMessage = `Error ${error.status}: ${errorBody.title}`;
      } else if (typeof errorBody === 'string' && errorBody.length > 0) {
        errorMessage = errorBody;
      } else {
        errorMessage = `Error ${error.status}: ${error.statusText}`;
      }
    }
    console.error(errorMessage);
    this._error.set(errorMessage); // Update the error signal
  }
}
