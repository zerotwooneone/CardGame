import { Injectable, inject, signal, Signal, WritableSignal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, tap, switchMap, filter, take } from 'rxjs/operators';
import { toObservable } from '@angular/core/rxjs-interop';
import { CardDto } from '../../../core/models/cardDto';

@Injectable({
  providedIn: 'any'
})
export class DeckService {
  private apiUrl = '/api/deck';
  private http = inject(HttpClient);
  private hardcodedDeckId = '00000000-0000-0000-0000-000000000001';

  private defaultDeckCache: CardDto[] | null = null;
  private appearanceIdToRankMap: Map<string, number> = new Map<string, number>();
  private deckLoadStatusWritableSignal: WritableSignal<boolean> = signal<boolean>(false);
  private isLoadingDeck: boolean = false;

  constructor() { }

  /**
   * Ensures the default deck is loaded, fetching it via HTTP if necessary.
   * Returns an observable that emits the deck once loaded.
   */
  ensureDeckLoaded(): Observable<CardDto[]> {
    if (this.defaultDeckCache !== null) {
      return of(this.defaultDeckCache);
    }

    if (this.isLoadingDeck) {
      return toObservable(this.deckLoadStatusWritableSignal).pipe(
        filter(loaded => loaded),
        take(1), 
        switchMap(() => of(this.defaultDeckCache!)) 
      );
    }

    this.isLoadingDeck = true;
    this.appearanceIdToRankMap.clear(); 

    return this.http.get<CardDto[]>(`${this.apiUrl}/${this.hardcodedDeckId}`).pipe(
      tap(deck => {
        this.defaultDeckCache = deck;
        deck.forEach(card => {
          this.appearanceIdToRankMap.set(card.appearanceId, card.rank);
        });
        this.isLoadingDeck = false;
        this.deckLoadStatusWritableSignal.set(true); 
      }),
      catchError(err => {
        this.isLoadingDeck = false;
        this.deckLoadStatusWritableSignal.set(false); 
        this.handleError(err); 
        return throwError(() => new Error('Failed to load default deck.')); 
      })
    );
  }

  /**
   * @deprecated Prefer ensureDeckLoaded() for robust loading and caching.
   * Fetches the default game deck from the backend. 
   * This method now uses the caching mechanism.
   */
  getDefaultDeck(): Observable<CardDto[]> {
    return this.ensureDeckLoaded();
  }

  /**
   * Synchronously returns the cached deck. Returns null if the deck hasn't been loaded yet.
   */
  public getLoadedDeckSnapshot(): CardDto[] | null {
    return this.defaultDeckCache;
  }

  /**
   * Synchronously returns the AppearanceId to Rank map. Returns an empty map if not loaded.
   */
  public getRankMapSnapshot(): Map<string, number> {
    return this.appearanceIdToRankMap;
  }

  /**
   * Gets the rank of a card by its AppearanceId from the cached map.
   * @param appearanceId The appearance ID of the card.
   * @returns The rank of the card, or undefined if not found.
   */
  public getRankByAppearanceId(appearanceId: string): number | undefined {
    return this.appearanceIdToRankMap.get(appearanceId);
  }

  /**
   * Returns a read-only Signal that emits the current deck load status (true for loaded, false otherwise)
   * and any subsequent status changes.
   */
  public getDeckLoadStatusSignal(): Signal<boolean> { 
    return this.deckLoadStatusWritableSignal.asReadonly(); 
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
        errorMessage = `Error ${error.status}: ${error.statusText}`;
      }
    }
    console.error(errorMessage); 
  }
}
