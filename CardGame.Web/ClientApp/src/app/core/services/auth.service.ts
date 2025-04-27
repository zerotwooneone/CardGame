// Example path: src/app/core/services/auth.service.ts
import {Inject, Injectable, PLATFORM_ID, signal, Signal, WritableSignal} from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { LoginRequest } from '../models/loginRequest';
import { LoginResponse } from '../models/loginResponse';
import {isPlatformBrowser} from '@angular/common';

@Injectable({
  providedIn: 'root' // Service available application-wide
})
export class AuthService {
  // Use relative path to API - configure base URL via proxy or interceptor
  private apiUrl = '/api/Auth';
  private isBrowser: boolean; // Flag to check if running in browser

  // --- Use Signal for logged-in status ---
  // Create a writable signal, initialized by checking storage (only if in browser)
  private loggedInStatusSignal: WritableSignal<boolean>;
  // Expose a readonly version for external consumption
  public isLoggedIn: Signal<boolean>;
  // --- End Signal usage ---

  // Inject PLATFORM_ID to detect environment
  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId); // Check platform on init
    // Initialize signal based on platform
    this.loggedInStatusSignal = signal<boolean>(this.isBrowser ? this.hasAuthInfo() : false);
    this.isLoggedIn = this.loggedInStatusSignal.asReadonly();
  }

  /**
   * Sends login request to the backend.
   * @param credentials - The user's login credentials.
   * @returns Observable of the LoginResponse.
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          // Only interact with localStorage in the browser
          if (this.isBrowser) {
            // **Added Checks:** Ensure properties exist before using them
            if (response?.playerId && response?.username) {
              localStorage.setItem('currentPlayerId', response.playerId);
              localStorage.setItem('currentUsername', response.username);
              this.loggedInStatusSignal.set(true); // Update signal value using .set()
              console.log('Login successful:', response);
            } else {
              console.error('Login response missing expected playerId or username:', response);
              this.loggedInStatusSignal.set(false); // Update signal value using .set()
              throw new Error('Login failed: Invalid response data from server.');
            }
          } else {
            // Handle server-side logic if necessary (e.g., setting state differently)
            // For now, just log success but don't touch localStorage
            if (response?.playerId && response?.username) {
              console.log('Login successful on server (no localStorage access):', response);
              // Note: Signal update might not persist correctly across SSR->browser transition
              // depending on hydration strategy. Usually, client re-evaluates on hydration.
              this.loggedInStatusSignal.set(true);
            } else {
              console.error('Login response missing expected playerId or username on server:', response);
              this.loggedInStatusSignal.set(false);
              throw new Error('Login failed: Invalid response data from server.');
            }
          }
        }),
        catchError(this.handleError) // Catch HTTP errors
      );
  }

  /**
   * Sends logout request to the backend.
   * @returns Observable indicating completion.
   */
  logout(): Observable<any> {
    // Only perform localStorage cleanup and signal update in the browser
    if (this.isBrowser) {
      return this.http.post(`${this.apiUrl}/logout`, {})
        .pipe(
          tap(() => {
            localStorage.removeItem('currentPlayerId');
            localStorage.removeItem('currentUsername');
            this.loggedInStatusSignal.set(false); // Update signal value using .set()
            console.log('Logout successful');
          }),
          catchError(this.handleError)
        );
    } else {
      // On the server, just return a completed observable as there's no client state to clear
      console.log('Logout called on server (no client state to clear).');
      this.loggedInStatusSignal.set(false); // Update server-side signal state if needed
      return of(null); // Return an observable that completes immediately
    }
  }

  /**
   * Simple check if authentication info exists in localStorage (browser only).
   */
  private hasAuthInfo(): boolean {
    // Only check localStorage in the browser
    if (this.isBrowser) {
      return !!localStorage.getItem('currentPlayerId');
    }
    return false; // Assume not authenticated on the server
  }

  /**
   * Basic error handler for HTTP requests.
   */
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'An unknown error occurred!';
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
    return throwError(() => new Error(errorMessage));
  }

  /**
   * Gets the stored Player ID (browser only).
   */
  getCurrentPlayerId(): string | null {
    if (this.isBrowser) {
      return localStorage.getItem('currentPlayerId');
    }
    return null; // No localStorage on server
  }

  // --- Added methods for username persistence (Platform Aware) ---

  private readonly USERNAME_STORAGE_KEY = 'lastUsername';
  private readonly REMEMBER_USERNAME_KEY = 'rememberUsernamePref';

  /** Gets the saved username from local storage (browser only). */
  getLastUsername(): string {
    if (this.isBrowser) {
      return localStorage.getItem(this.USERNAME_STORAGE_KEY) || '';
    }
    return ''; // Default on server
  }

  /** Saves the username to local storage (browser only). */
  setLastUsername(username: string): void {
    if (this.isBrowser) {
      if (username) {
        localStorage.setItem(this.USERNAME_STORAGE_KEY, username);
      } else {
        localStorage.removeItem(this.USERNAME_STORAGE_KEY);
      }
    }
  }

  /** Clears the saved username from local storage (browser only). */
  clearLastUsername(): void {
    if (this.isBrowser) {
      localStorage.removeItem(this.USERNAME_STORAGE_KEY);
    }
  }

  /** Gets the user's preference for remembering the username (browser only). */
  getRememberUsernamePreference(): boolean {
    if (this.isBrowser) {
      // Default to true if not set
      return localStorage.getItem(this.REMEMBER_USERNAME_KEY) !== 'false';
    }
    return true; // Default preference on server (or could be false)
  }

  /** Sets the user's preference for remembering the username (browser only). */
  setRememberUsernamePreference(remember: boolean): void {
    if (this.isBrowser) {
      localStorage.setItem(this.REMEMBER_USERNAME_KEY, remember.toString());
      // If preference is set to false, clear the saved username
      if (!remember) {
        this.clearLastUsername();
      }
    }
  }
  // --- End added methods ---
}
