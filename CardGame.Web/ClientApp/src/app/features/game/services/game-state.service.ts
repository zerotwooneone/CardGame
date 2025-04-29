import { Injectable, signal, WritableSignal, Signal, OnDestroy, effect, Injector, computed } from '@angular/core';
import {Observable, Subject, Subscription } from 'rxjs';

// Import core services and models
import { SignalrService, ConnectionState } from '../../../core/services/signalr.service'; // Adjust path
import { AuthService } from '../../../core/services/auth.service';
import {SpectatorGameStateDto} from '../../../core/models/spectatorGameStateDto';
import {CardDto} from '../../../core/models/cardDto'; // Adjust path


@Injectable({
  // Provide locally within the game feature or root if needed elsewhere
  providedIn: 'any'
})
export class GameStateService implements OnDestroy {

  // --- Private Writable Signals holding the state ---
  private spectatorStateSignal: WritableSignal<SpectatorGameStateDto | null> = signal(null);
  private playerHandSignal: WritableSignal<CardDto[]> = signal([]);

  // --- Public Readonly Signals for consumption ---
  public spectatorState: Signal<SpectatorGameStateDto | null> = this.spectatorStateSignal.asReadonly();
  public playerHand: Signal<CardDto[]> = this.playerHandSignal.asReadonly();

  // --- Computed Signals for derived state ---
  public gameId: Signal<string | null> = computed(() => this.spectatorState()?.gameId ?? null);
  public currentTurnPlayerId: Signal<string | null> = computed(() => this.spectatorState()?.currentTurnPlayerId ?? null);
  public isMyTurn: Signal<boolean> = computed(() => {
    const myId = this.authService.getCurrentPlayerId();
    const currentTurnId = this.currentTurnPlayerId();
    return !!myId && !!currentTurnId && myId === currentTurnId;
  });
  public gamePhase: Signal<string | null> = computed(() => this.spectatorState()?.gamePhase ?? null);

  // --- Observables for transient events ---
  // Declare properties first
  public readonly opponentHandRevealed$: Observable<{ opponentId: string, revealedCard: CardDto }>;
  public readonly playerGuessed$: Observable<{ guesserId: string, targetId: string, guessedCardTypeValue: number, wasCorrect: boolean }>;
  public readonly playersComparedHands$: Observable<{ player1Id: string, player1CardTypeValue: number, player2Id: string, player2CardTypeValue: number, loserId: string | null }>;
  public readonly playerDiscarded$: Observable<{ targetPlayerId: string, discardedCard: CardDto }>;
  public readonly cardsSwapped$: Observable<{ player1Id: string, player2Id: string }>;
  public readonly roundWinnerAnnounced$: Observable<{ winnerId: string | null, reason: string, finalHands: { [playerId: string]: number | null } }>;
  public readonly gameWinnerAnnounced$: Observable<{ winnerId: string }>;

  // --- Subscriptions ---
  private spectatorStateSubscription?: Subscription;
  private playerHandSubscription?: Subscription;
  // private connectionSubscription?: Subscription; // Removed: Replaced by effect

  constructor(
    private signalrService: SignalrService,
    private authService: AuthService, // To check current player ID
    private injector: Injector // Injector for effect context
  ) {
    // --- Assign Observables in constructor ---
    this.opponentHandRevealed$ = this.signalrService.opponentHandRevealReceived$;
    this.playerGuessed$ = this.signalrService.playerGuessedReceived$;
    this.playersComparedHands$ = this.signalrService.playersComparedHandsReceived$;
    this.playerDiscarded$ = this.signalrService.playerDiscardedReceived$;
    this.cardsSwapped$ = this.signalrService.cardsSwappedReceived$;
    this.roundWinnerAnnounced$ = this.signalrService.roundWinnerReceived$;
    this.gameWinnerAnnounced$ = this.signalrService.gameWinnerReceived$;
    // --- End Observable Assignment ---

    this.setupSubscriptions();

    // --- Use effect to react to isLoggedIn signal changes ---
    effect(() => {
      const isLoggedIn = this.authService.isLoggedIn();
      // Accessing isBrowser directly from signalrService
      const isBrowser = this.signalrService.isBrowser; // Use public readonly property
      if (!isLoggedIn && isBrowser) {
        console.log('User logged out (detected by signal), stopping SignalR connections.');
        Promise.all([
          this.stopNotificationConnection(),
          this.stopGameConnection()
        ]).catch(err => console.error("Error stopping SignalR connections on logout:", err));
      }
    }, { injector: this.injector });
    // --- End effect usage ---

    // --- Use effect to react to gameConnectionState signal changes ---
    effect(() => {
      const state = this.signalrService.gameConnectionState(); // Read the signal
      if (state === ConnectionState.Disconnected && this.signalrService.isBrowser) {
        console.log('GameStateService: GameHub disconnected (detected by signal), clearing state.');
        this.clearState();
      }
      // Could add logic here for ConnectionState.Reconnected to rejoin groups if needed
    }, { injector: this.injector }); // Provide injector context
    // --- End effect usage ---
  }

  ngOnDestroy(): void {
    this.unsubscribeAll();
    // Effects tied to the injector are automatically cleaned up
  }

  /** Sets up subscriptions to SignalR service observables */
  private setupSubscriptions(): void {
    // Only subscribe if in the browser
    if (!this.signalrService.isBrowser) {
      return;
    }

    // --- Removed connectionSubscription setup ---
    // this.connectionSubscription = this.signalrService.gameConnectionState$.subscribe(state => { ... });

    // Subscribe to spectator state updates
    this.spectatorStateSubscription = this.signalrService.spectatorGameStateReceived$.subscribe(
      state => {
        console.log('GameStateService: Received spectator state update.');
        this.spectatorStateSignal.set(state);
      }
    );

    // Subscribe to player hand updates
    this.playerHandSubscription = this.signalrService.playerHandReceived$.subscribe(
      hand => {
        console.log('GameStateService: Received player hand update.');
        this.playerHandSignal.set(hand);
      }
    );
  }

  /** Clears all current game state */
  public clearState(): void {
    this.spectatorStateSignal.set(null);
    this.playerHandSignal.set([]);
  }

  /** Unsubscribes from all observables */
  private unsubscribeAll(): void {
    this.spectatorStateSubscription?.unsubscribe();
    this.playerHandSubscription?.unsubscribe();
    // this.connectionSubscription?.unsubscribe(); // Removed
  }

  /**
   * Connects to the Game Hub and joins the specified game group.
   */
  public async connectToGame(gameId: string): Promise<void> {
    // Ensure connection is attempted only once if already connecting/connected
    if (this.signalrService.gameConnectionState() === ConnectionState.Connected ||
      this.signalrService.gameConnectionState() === ConnectionState.Connecting) {
      // If connected, ensure group is joined (might have reconnected)
      if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
        await this.signalrService.joinGameGroup(gameId);
      }
      return;
    }

    await this.signalrService.startGameConnection();
    // Check state *after* attempting connection
    if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
      await this.signalrService.joinGameGroup(gameId);
    } else {
      console.error("GameStateService: Cannot join game group, connection not established after attempt.");
    }
  }

  /**
   * Leaves the specified game group and potentially stops the connection.
   */
  public async disconnectFromGame(gameId: string): Promise<void> {
    // Only leave group if connected
    if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
      await this.signalrService.leaveGameGroup(gameId);
    }
    // Consider stopping connection only if no other components need it
    // await this.signalrService.stopGameConnection();
  }

  // Placeholder methods for stopping connections if needed by the effect
  private async stopNotificationConnection(): Promise<void> {
    await this.signalrService.stopNotificationConnection();
  }
  private async stopGameConnection(): Promise<void> {
    await this.signalrService.stopGameConnection();
  }

}

