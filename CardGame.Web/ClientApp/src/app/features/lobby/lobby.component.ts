// Example path: src/app/features/lobby/lobby.component.ts
import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  ChangeDetectionStrategy,
  WritableSignal,
  signal,
  ChangeDetectorRef
} from '@angular/core'; // Added OnDestroy, ChangeDetectionStrategy
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject } from 'rxjs'; // Added Observable, Subject, Subscription
import { takeUntil } from 'rxjs/operators'; // Added takeUntil

// Import services needed
import { AuthService } from '../../core/services/auth.service'; // Adjust path if needed
import { GameLobbyService } from './services/game-lobby.service'; // Import the lobby service
import { SignalrService } from '../../core/services/signalr.service';
import {GameSummaryDto} from '../../core/models/gameSummaryDto';
import {MatDialog, MatDialogModule} from '@angular/material/dialog';
import {UserInfo} from '../../core/models/userInfo';
import {GameActionService} from '../game/services/game-action.service';
import {CreateGameDialogResult} from './models/createGameDialogResult';
import {CreateGameDialogComponent} from './create-game-dialog/create-game-dialog.component'; // Still needed to start connection


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
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './lobby.component.html',
  styleUrls: ['./lobby.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LobbyComponent implements OnInit, OnDestroy {
  isLoading = false; // Keep for actions like creating game
  errorMessage: string | null = null;

  // --- State Signals ---
  // Store invitations received via SignalR
  invitedGames: WritableSignal<Array<{ gameId: string; creatorName: string }>> = signal([]);
  // Store known friends (loaded from/saved to localStorage)
  knownFriends: WritableSignal<UserInfo[]> = signal([]);

  private destroy$ = new Subject<void>();
  private readonly MAX_KNOWN_FRIENDS = 100;
  private readonly KNOWN_FRIENDS_STORAGE_KEY = 'knownFriendCodes';

  // Inject services
  private router = inject(Router);
  private authService = inject(AuthService);
  private signalrService = inject(SignalrService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog); // Inject MatDialog
  private cdr = inject(ChangeDetectorRef); // Inject ChangeDetectorRef
  private gameActionService = inject(GameActionService); // Inject for Create Game (placeholder)

  constructor() { }

  ngOnInit(): void {
    this.loadKnownFriends();
    this.connectToNotifications();
    // No longer loading available games from API
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    // Optional: Disconnect SignalR here if lobby is the only place it's needed
    // this.signalrService.stopNotificationConnection();
  }

  // --- Known Friends Management ---
  private loadKnownFriends(): void {
    if (this.signalrService.isBrowser) { // Check if running in browser
      try {
        const storedJson = localStorage.getItem(this.KNOWN_FRIENDS_STORAGE_KEY);
        if (storedJson) {
          const storedFriends: UserInfo[] = JSON.parse(storedJson);
          // Basic validation might be good here
          if (Array.isArray(storedFriends)) {
            this.knownFriends.set(storedFriends);
          } else {
            localStorage.removeItem(this.KNOWN_FRIENDS_STORAGE_KEY); // Clear invalid data
          }
        }
      } catch (e) {
        console.error("Error loading known friends from localStorage:", e);
        localStorage.removeItem(this.KNOWN_FRIENDS_STORAGE_KEY); // Clear potentially corrupt data
      }
    }
  }

  private addKnownFriend(friendInfo: UserInfo): void {
    if (!this.signalrService.isBrowser) return;

    this.knownFriends.update(currentFriends => {
      // Avoid duplicates based on playerId
      if (currentFriends.some(f => f.playerId === friendInfo.playerId)) {
        return currentFriends;
      }
      // Add new friend to the start (most recent)
      const updatedFriends = [friendInfo, ...currentFriends];
      // Limit size
      if (updatedFriends.length > this.MAX_KNOWN_FRIENDS) {
        updatedFriends.pop(); // Remove the oldest (last element)
      }
      // Save to localStorage
      try {
        localStorage.setItem(this.KNOWN_FRIENDS_STORAGE_KEY, JSON.stringify(updatedFriends));
      } catch (e) {
        console.error("Error saving known friends to localStorage:", e);
      }
      return updatedFriends;
    });
  }
  // --- End Known Friends Management ---


  connectToNotifications(): void {
    // Start the notification hub connection via the SignalR service
    this.signalrService.startNotificationConnection()
      .catch(err => {
        console.error("Error starting notification connection from Lobby:", err);
        this.snackBar.open('Could not connect for game invitations.', 'Close', { duration: 3000 });
      });

    // Subscribe to invitations
    this.signalrService.gameInvitationReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ gameId, creatorName }) => {
        console.log(`LobbyComponent received invitation: GameId=${gameId}, Creator=${creatorName}`);

        // Add to signal if not already present
        this.invitedGames.update(currentInvites => {
          if (currentInvites.some(inv => inv.gameId === gameId)) {
            return currentInvites; // Already have this invite
          }
          return [...currentInvites, { gameId, creatorName }];
        });

        // Show snackbar notification
        const snackBarRef = this.snackBar.open(
          `${creatorName} invited you to a game!`,
          'Join',
          { duration: 10000 }
        );
        snackBarRef.onAction().subscribe(() => this.joinGame(gameId));
        this.cdr.markForCheck(); // Trigger change detection as this is outside normal flow
      });
  }

  openCreateGameDialog(): void {
    const dialogRef = this.dialog.open<CreateGameDialogComponent, { knownFriends: UserInfo[] }, CreateGameDialogResult | undefined>(
       CreateGameDialogComponent ,
      {
        width: '450px',
        data: { knownFriends: this.knownFriends() }, // Pass current known friends
        disableClose: true // Prevent closing by clicking outside
      }
    );

    dialogRef.afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        if (result?.selectedOpponentIds && result.selectedOpponentIds.length > 0) {
          // User confirmed creation with selected opponents
          const opponentIds = result.selectedOpponentIds;
          const myId = this.authService.getCurrentPlayerId();

          if (!myId) {
            this.showSnackBar('Error: Could not identify current user.', 5000);
            return;
          }

          const allPlayerIds = [myId, ...opponentIds];

          // Basic validation (should match command/backend validation)
          if (allPlayerIds.length < 2 || allPlayerIds.length > 4) {
            this.showSnackBar(`Error: Games must have 2-4 players (found ${allPlayerIds.length}).`, 5000);
            return;
          }
          if (new Set(allPlayerIds).size !== allPlayerIds.length) {
            this.showSnackBar(`Error: Duplicate player IDs selected.`, 5000);
            return;
          }

          // Update known friends list with any newly validated ones from the dialog
          if (result.newlyValidatedFriendCodes?.length) {
            result.newlyValidatedFriendCodes.forEach(code => {
              try {
                const friendInfo: UserInfo = JSON.parse(code);
                // Add basic validation if needed before adding
                if(friendInfo.playerId && friendInfo.username) {
                  this.addKnownFriend(friendInfo);
                }
              } catch (e) {
                console.warn("Dialog returned invalid friend code string:", code);
              }
            });
          }

          // Call backend to create game (placeholder for API call)
          this.initiateGameCreation(allPlayerIds);

        } else {
          // User cancelled
          console.log('Create game cancelled');
        }
      });
  }

  initiateGameCreation(playerIds: string[]): void {
    this.isLoading = true;
    this.errorMessage = null;
    console.log("TODO: Call backend API to create game with Player IDs:", playerIds);

    // --- Placeholder for actual backend call ---
    // const command = new CreateGameCommand(playerIds.map(id => Guid.parse(id)), Guid.parse(this.authService.getCurrentPlayerId()!), null);
    // this.mediator.send(command)... or this.gameActionService.createGame(...)
    setTimeout(() => { // Simulate API call
      this.isLoading = false;
      // On success, maybe navigate or wait for SignalR update?
      // For now, just show success message
      this.showSnackBar(`Game creation initiated with ${playerIds.length} players.`, 3000);
      // Maybe clear invitations list?
      // this.invitedGames.set([]);
      this.cdr.markForCheck();
    }, 1500);
    // --- End Placeholder ---
  }


  joinGame(gameId: string): void {
    // Navigate to the game view
    this.router.navigate(['/game', gameId]);
  }

  logout(): void {
    this.authService.logout()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.router.navigate(['/auth/login']), // Redirect to login after logout
        error: (err) => this.showSnackBar(`Logout failed: ${err.message}`, 3000)
      });
  }

  showSnackBar(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Close', { duration });
  }

  // --- TrackBy Functions ---
  trackByIndex(index: number, item: any): number { return index; }
  trackGameInvite(index: number, item: { gameId: string; creatorName: string }): string { return item.gameId; }

}
