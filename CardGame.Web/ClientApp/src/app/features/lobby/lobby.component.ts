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
import { finalize, Subject} from 'rxjs'; // Added Observable, Subject, Subscription
import { takeUntil } from 'rxjs/operators'; // Added takeUntil

// Import services needed
import { GameLobbyService } from './services/game-lobby.service'; // Import the lobby service
import { SignalrService } from '@core/services/signalr.service';
import { AuthService } from '@features/auth/services/auth.service';
import {GameSummaryDto} from '../game/models/gameSummaryDto';
import {MatDialog, MatDialogModule} from '@angular/material/dialog';
import {UserInfo} from '../auth/models/userInfo';
import {GameActionService} from '../game/services/game-action.service';
import {CreateGameDialogResult} from './models/createGameDialogResult';
import {CreateGameDialogComponent} from './create-game-dialog/create-game-dialog.component';
import {CreateGameDialogData} from './models/createGameDialogData';
import {CreateGameRequestDto} from '../game/models/createGameRequestDto'; // Still needed to start connection
import { DeckService } from '@features/game/services/deck.service'; // Added DeckService import


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
    MatDialogModule // Add MatDialogModule
  ],
  templateUrl: './lobby.component.html',
  styleUrls: ['./lobby.component.scss'],
  // Use OnPush for better performance when relying on observables/signals
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LobbyComponent implements OnInit, OnDestroy {
  isLoading = signal(false); // Use signal for loading state
  errorMessage = signal<string | null>(null); // Use signal for error state

  // --- State Signals ---
  invitedGames: WritableSignal<Array<{ gameId: string; creatorName: string }>> = signal([]);
  knownFriends: WritableSignal<UserInfo[]> = signal([]);

  private destroy$ = new Subject<void>();
  private readonly MAX_KNOWN_FRIENDS = 100;
  private readonly KNOWN_FRIENDS_STORAGE_KEY = 'knownFriendCodes';

  // Inject services
  private router = inject(Router);
  private authService = inject(AuthService);
  private signalrService = inject(SignalrService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private cdr = inject(ChangeDetectorRef);
  private gameActionService = inject(GameActionService); // Inject GameActionService
  private deckService = inject(DeckService); // Inject DeckService

  constructor() { }

  ngOnInit(): void {
    this.loadKnownFriends();
    this.connectToNotifications();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // --- Known Friends Management ---
  private loadKnownFriends(): void {
    if (this.signalrService.isBrowser) {
      try {
        const storedJson = localStorage.getItem(this.KNOWN_FRIENDS_STORAGE_KEY);
        if (storedJson) {
          const storedFriends: UserInfo[] = JSON.parse(storedJson);
          if (Array.isArray(storedFriends)) {
            this.knownFriends.set(storedFriends);
          } else { localStorage.removeItem(this.KNOWN_FRIENDS_STORAGE_KEY); }
        }
      } catch (e) {
        console.error("Error loading known friends from localStorage:", e);
        localStorage.removeItem(this.KNOWN_FRIENDS_STORAGE_KEY);
      }
    }
  }

  private addKnownFriend(friendInfo: UserInfo): void {
    if (!this.signalrService.isBrowser) return;
    this.knownFriends.update(currentFriends => {
      if (currentFriends.some(f => f.playerId === friendInfo.playerId)) return currentFriends;
      const updatedFriends = [friendInfo, ...currentFriends];
      if (updatedFriends.length > this.MAX_KNOWN_FRIENDS) updatedFriends.pop();
      try { localStorage.setItem(this.KNOWN_FRIENDS_STORAGE_KEY, JSON.stringify(updatedFriends)); }
      catch (e) { console.error("Error saving known friends to localStorage:", e); }
      return updatedFriends;
    });
  }
  // --- End Known Friends Management ---


  connectToNotifications(): void {
    this.signalrService.startNotificationConnection()
      .catch(err => {
        console.error("Error starting notification connection from Lobby:", err);
        this.snackBar.open('Could not connect for game invitations.', 'Close', { duration: 3000 });
      });

    this.signalrService.gameInvitationReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ gameId, creatorName }) => {
        console.log(`LobbyComponent received invitation: GameId=${gameId}, Creator=${creatorName}`);
        this.invitedGames.update(currentInvites => {
          if (currentInvites.some(inv => inv.gameId === gameId)) return currentInvites;
          return [...currentInvites, { gameId, creatorName }];
        });
        const snackBarRef = this.snackBar.open(`${creatorName} invited you to a game!`, 'Join', { duration: 10000 });
        snackBarRef.onAction().subscribe(() => this.joinGame(gameId));
        this.cdr.markForCheck();
      });
  }

  openCreateGameDialog(): void {
    const dialogData: CreateGameDialogData = { knownFriends: this.knownFriends() };

    const dialogRef = this.dialog.open<CreateGameDialogComponent, CreateGameDialogData, CreateGameDialogResult | undefined>(
      CreateGameDialogComponent,
      { width: '450px', data: dialogData, disableClose: true }
    );

    dialogRef.afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        if (result?.selectedOpponentIds && result.selectedOpponentIds.length > 0 && result.deckId) { 
          const myId = this.authService.getCurrentPlayerId();
          if (!myId) {
            this.showSnackBar('Error: Could not identify current user.', 5000);
          }

          const allPlayerIds = [myId, ...result.selectedOpponentIds];

          if (allPlayerIds.length < 2 || allPlayerIds.length > 4) {
            this.showSnackBar(`Error: Games must have 2-4 players (found ${allPlayerIds.length}).`, 5000);
            return;
          }
          if (new Set(allPlayerIds).size !== allPlayerIds.length) {
            this.showSnackBar(`Error: Duplicate player IDs selected.`, 5000);
            return;
          }

          // Update known friends list
          if (result.newlyValidatedFriendCodes?.length) {
            result.newlyValidatedFriendCodes.forEach(code => {
              try {
                const friendInfo: UserInfo = JSON.parse(code);
                if(friendInfo.playerId && friendInfo.username) { this.addKnownFriend(friendInfo); }
              } catch (e) { console.warn("Dialog returned invalid friend code string:", code); }
            });
          }

          // Set the selected deck ID in the DeckService
          this.deckService.setSelectedDeckId(result.deckId);

          // Call backend to create game
          this.createGame(allPlayerIds.filter(id => id !== null) as string[], result.deckId); 

        } else {
          console.log('Create game cancelled or deckId missing');
        }
      });
  }

  // --- Updated: Method to call the backend API ---
  createGame(playerIds: string[], deckId: string): void { 
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const payload: CreateGameRequestDto = {
      PlayerIds: playerIds, 
      DeckId: deckId 
      // TokensToWin: null // Optional: Get from UI if needed
    };

    this.gameActionService.createGame(payload) 
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading.set(false)) 
      )
      .subscribe({
        next: (newGameId) => {
          this.showSnackBar(`Game ${newGameId.substring(0,8)}... created successfully!`, 3000);
          // Navigate to the newly created game
          this.router.navigate(['/game', newGameId]);
          // Clear invitations list? Or let SignalR update handle it?
          // this.invitedGames.set([]);
        },
        error: (err) => {
          this.errorMessage.set(err.message || 'Failed to create game.');
          this.showSnackBar(`Error: ${this.errorMessage()}`, 5000);
          this.cdr.markForCheck(); 
        }
      });
  }
  // --- End Update ---


  joinGame(gameId: string): void {
    // Navigate to the game view
    this.router.navigate(['/game', gameId]);
  }

  logout(): void {
    this.authService.logout()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.router.navigate(['/auth/login']), 
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
