// Example path: src/app/features/lobby/lobby.component.ts
import { Component, OnInit, OnDestroy, inject, ChangeDetectionStrategy } from '@angular/core'; // Added OnDestroy, ChangeDetectionStrategy
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Observable, Subject, Subscription } from 'rxjs'; // Added Observable, Subject, Subscription
import { takeUntil } from 'rxjs/operators'; // Added takeUntil

// Import services needed
import { AuthService } from '../../core/services/auth.service'; // Adjust path if needed
import { GameLobbyService } from './services/game-lobby.service'; // Import the lobby service
import { SignalrService } from '../../core/services/signalr.service';
import {GameSummaryDto} from '../../core/models/gameSummaryDto'; // Still needed to start connection


@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './lobby.component.html',
  styleUrls: ['./lobby.component.scss'],
  // Use OnPush for better performance when relying on observables/signals
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LobbyComponent implements OnInit, OnDestroy {
  isLoading = false; // Keep for loading state during initial fetch maybe
  // Use observable directly from the service
  availableGames$: Observable<GameSummaryDto[]>;
  errorMessage: string | null = null;

  private destroy$ = new Subject<void>(); // For unsubscribing

  // Inject services
  private router = inject(Router);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);
  private gameLobbyService = inject(GameLobbyService); // Inject the service
  private signalrService = inject(SignalrService); // Inject SignalR service

  constructor() {
    // Assign the observable from the service
    this.availableGames$ = this.gameLobbyService.games$;
  }

  ngOnInit(): void {
    this.loadAvailableGames(); // Trigger initial load
    this.connectToNotifications(); // Start SignalR connection
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    // Optional: Disconnect SignalR here if lobby is the only place it's needed
    // this.signalrService.stopNotificationConnection();
  }


  loadAvailableGames(): void {
    this.isLoading = true; // Indicate loading
    this.errorMessage = null;
    // Call the service method to trigger the load
    // The result will be pushed to the availableGames$ observable
    this.gameLobbyService.loadAvailableGames();

    // We might not need isLoading here anymore if the template handles async pipe loading state
    // For simplicity, let's assume loading finishes quickly or template handles it
    // If loadAvailableGames returned an observable, we could subscribe here to set isLoading false
    // Since it's pushing to a subject, we rely on the initial state or add more complex loading logic
    this.isLoading = false; // Remove this if template handles loading state with async pipe
  }

  connectToNotifications(): void {
    // Start the notification hub connection via the SignalR service
    this.signalrService.startNotificationConnection()
      .catch(err => {
        console.error("Error starting notification connection from Lobby:", err);
        this.snackBar.open('Could not connect for game invitations.', 'Close', { duration: 3000 });
      });
    // Subscription to invitations is now handled within GameLobbyService
  }

  navigateToCreateGame(): void {
    this.router.navigate(['/create-game']); // Adjust route as needed
  }

  joinGame(gameId: string): void {
    // Navigate to the game view
    this.router.navigate(['/game', gameId]);
  }

  logout(): void {
    this.authService.logout()
      .pipe(takeUntil(this.destroy$)) // Unsubscribe on destroy
      .subscribe({
        next: () => this.router.navigate(['/auth/login']), // Redirect to login after logout
        error: (err) => this.snackBar.open(`Logout failed: ${err.message}`, 'Close', { duration: 3000 })
      });
  }
}

