// Example path: src/app/features/lobby/services/game-lobby.service.ts
import { Injectable, inject, OnDestroy } from '@angular/core'; // Added OnDestroy
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of, Subject, Subscription, takeUntil } from 'rxjs'; // Import of, Subject, Subscription, takeUntil
import { catchError, delay, map, tap } from 'rxjs/operators';
import { SignalrService } from '../../../core/services/signalr.service'; // Import SignalrService
import { MatSnackBar } from '@angular/material/snack-bar'; // Import MatSnackBar for notifications
import { Router } from '@angular/router';
import {GameSummaryDto} from '../../game/models/gameSummaryDto'; // Import Router for navigation

@Injectable({
  // Provided locally within the lobby feature routing or component if lazy loaded,
  // or providedIn: 'root' if used widely and not feature-specific.
  // Let's assume it might be specific to the lobby feature for now.
  providedIn: 'any' // Or specific module if using modules
})
export class GameLobbyService implements OnDestroy {
  // Use relative path to API - configure base URL via proxy or interceptor
  // TODO: Define this backend endpoint (e.g., GET /api/Game/lobby or /api/Lobby)
  private apiUrl = '/api/Game/lobby'; // Example API endpoint

  private http = inject(HttpClient);
  private signalrService = inject(SignalrService); // Inject SignalrService
  private snackBar = inject(MatSnackBar); // Inject MatSnackBar
  private router = inject(Router); // Inject Router

  // Subject to handle component destruction for unsubscribing
  private destroy$ = new Subject<void>();

  // Observable stream for the list of games, allowing refresh
  private gamesSubject = new Subject<GameSummaryDto[]>();
  public games$: Observable<GameSummaryDto[]> = this.gamesSubject.asObservable();

  constructor() {
    this.subscribeToInvitations();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Fetches the list of available games from the backend API.
   * Pushes the result to the games$ observable.
   */
  loadAvailableGames(): void {
    // TODO: Replace dummy data with actual HTTP call when backend endpoint exists
    /*
    this.http.get<GameSummaryDto[]>(this.apiUrl)
      .pipe(
          takeUntil(this.destroy$), // Unsubscribe when component/service is destroyed
          catchError(this.handleError)
      ).subscribe(games => {
          this.gamesSubject.next(games);
      });
    */

    // --- Return Dummy Data for now ---
    const dummyGames: GameSummaryDto[] = [
      { gameId: 'a1b2c3d4-e5f6-7890-1234-567890abcdef', playerCount: 2, status: 'InProgress', createdBy: 'Alice' },
      { gameId: 'b2c3d4e5-f6a7-8901-2345-67890abcdef0', playerCount: 3, status: 'Waiting', createdBy: 'Bob' },
      { gameId: 'c3d4e5f6-a7b8-9012-3456-7890abcdef01', playerCount: 4, status: 'InProgress', createdBy: 'Charlie' },
    ];
    // Simulate network delay and push dummy data using 'of'
    of(dummyGames).pipe(
      delay(500),
      takeUntil(this.destroy$) // Ensure even dummy observable unsubscribes
    ).subscribe(games => {
      this.gamesSubject.next(games);
    });
    // --- End Dummy Data ---
  }

  /**
   * Subscribes to game invitations received via SignalR.
   */
  private subscribeToInvitations(): void {
    this.signalrService.gameInvitationReceived$
      .pipe(takeUntil(this.destroy$)) // Auto-unsubscribe when service is destroyed
      .subscribe(({ gameId, creatorName }) => {
        console.log(`GameLobbyService received invitation: GameId=${gameId}, Creator=${creatorName}`);
        // Show a snackbar notification
        const snackBarRef = this.snackBar.open(
          `${creatorName} invited you to a game!`,
          'Join', // Action button text
          { duration: 10000 } // Keep open for 10 seconds
        );

        // Handle the action when the 'Join' button is clicked
        snackBarRef.onAction().subscribe(() => {
          this.joinGame(gameId);
        });

        // Optionally refresh the game list when an invitation is received
        // this.loadAvailableGames(); // Consider if this is needed or desired behavior
      });
  }

  /**
   * Navigates the user to the specified game screen.
   * @param gameId The ID of the game to join/spectate.
   */
  private joinGame(gameId: string): void {
    this.router.navigate(['/game', gameId]);
  }


  /**
   * Basic error handler for HTTP requests.
   * Consider moving to a shared utility or interceptor.
   */
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An unknown error occurred fetching lobby games!';
    if (error.error instanceof ErrorEvent) {
      // Client-side errors
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side errors
      const errorBody = error.error;
      if (typeof errorBody === 'string') {
        errorMessage = errorBody;
      } else if (errorBody && typeof errorBody.detail === 'string') {
        errorMessage = errorBody.detail;
      } else if (errorBody && typeof errorBody.title === 'string') {
        errorMessage = errorBody.title;
      } else {
        errorMessage = `Server returned code ${error.status}, error message is: ${error.message}`;
      }
    }
    console.error(errorMessage);
    // Push error state to subject? Or handle directly in component?
    // this.gamesSubject.error(errorMessage); // Example
    return throwError(() => new Error(errorMessage));
  }
}
