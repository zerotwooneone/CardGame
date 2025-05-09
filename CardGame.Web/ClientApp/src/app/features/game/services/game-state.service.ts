import {Injectable, signal, WritableSignal, Signal, OnDestroy, effect, Injector, computed, inject} from '@angular/core';
import {Observable, Subject, Subscription } from 'rxjs';
import { SignalrService, ConnectionState } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import {SpectatorGameStateDto} from '../../../core/models/spectatorGameStateDto';
import {CardDto} from '../../../core/models/cardDto';
import {GameActionService} from './game-action.service';
import {PlayerGameStateDto} from '../../../core/models/playerGameStateDto';
import {RoundEndSummaryDto} from '../../../core/models/roundEndSummaryDto';


@Injectable({
  // Provide locally within the game feature or root if needed elsewhere
  providedIn: 'any'
})
export class GameStateService implements OnDestroy {

  // --- Private Writable Signals holding the state ---
  private spectatorStateSignal: WritableSignal<SpectatorGameStateDto | null> = signal(null);
  private playerHandSignal: WritableSignal<CardDto[]> = signal([]);
  private isLoadingState: WritableSignal<boolean> = signal(false);
  private errorState: WritableSignal<string | null> = signal(null);

  // --- Public Readonly Signals for consumption ---
  public spectatorState: Signal<SpectatorGameStateDto | null> = this.spectatorStateSignal.asReadonly();
  public playerHand: Signal<CardDto[]> = this.playerHandSignal.asReadonly();
  public isLoading: Signal<boolean> = this.isLoadingState.asReadonly();
  public error: Signal<string | null> = this.errorState.asReadonly();


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
  // This observable is correctly typed to RoundEndSummaryDto
  public readonly roundWinnerAnnounced$: Observable<RoundEndSummaryDto>;
  public readonly gameWinnerAnnounced$: Observable<{ winnerId: string }>;
  public readonly cardEffectFizzled$: Observable<{ actorId: string, cardTypeValue: number, targetId: string, reason: string }>;

  // --- Subscriptions ---
  private spectatorStateSubscription?: Subscription;
  private playerHandSubscription?: Subscription;

  // Inject services using inject() for consistency or keep constructor injection
  private signalrService = inject(SignalrService);
  private authService = inject(AuthService);
  private gameActionService = inject(GameActionService); // Inject GameActionService
  private injector = inject(Injector);

  constructor() {
    // Assign Observables in constructor
    // This ensures this.signalrService is initialized before being accessed
    this.opponentHandRevealed$ = this.signalrService.opponentHandRevealReceived$;
    this.playerGuessed$ = this.signalrService.playerGuessedReceived$;
    this.playersComparedHands$ = this.signalrService.playersComparedHandsReceived$;
    this.playerDiscarded$ = this.signalrService.playerDiscardedReceived$;
    this.cardsSwapped$ = this.signalrService.cardsSwappedReceived$;
    // Correctly assigns the observable from SignalrService
    this.roundWinnerAnnounced$ = this.signalrService.roundSummaryReceived$; // Previously roundWinnerReceived$
    this.gameWinnerAnnounced$ = this.signalrService.gameWinnerReceived$;
    this.cardEffectFizzled$ = this.signalrService.cardEffectFizzledReceived$;

    this.setupEffects();
  }

  ngOnDestroy(): void {
    this.unsubscribeAll();
    // Effects tied to the injector are automatically cleaned up
    const currentId = this.gameId();
    if (currentId) {
      this.disconnectFromGame(currentId);
    }
  }

  /** Sets up effects to react to signal changes */
  private setupEffects(): void {
    // Effect to react to isLoggedIn signal changes
    effect(() => {
      const isLoggedIn = this.authService.isLoggedIn();
      const isBrowser = this.signalrService.isBrowser; // Use public readonly property
      if (!isLoggedIn && isBrowser) {
        console.log('User logged out (detected by signal), stopping SignalR connections.');
        Promise.all([
          this.stopNotificationConnection(),
          this.stopGameConnection()
        ]).catch(err => console.error("Error stopping SignalR connections on logout:", err));
      }
    }, { injector: this.injector });

    // Effect to react to gameConnectionState signal changes
    effect(() => {
      const state = this.signalrService.gameConnectionState(); // Read the signal
      if (state === ConnectionState.Disconnected && this.signalrService.isBrowser) {
        console.log('GameStateService: GameHub disconnected (detected by signal), clearing state.');
        this.clearState(); // Clear state on disconnect
      }
      // Could add logic here for ConnectionState.Reconnected to rejoin groups if needed
    }, { injector: this.injector });
  }


  /** Sets up subscriptions to SignalR service observables */
  private setupSignalRSubscriptions(): void {
    // Clear existing subscriptions first
    this.unsubscribeAll();

    if (!this.signalrService.isBrowser) {
      return;
    }

    // Subscribe to spectator state updates
    this.spectatorStateSubscription = this.signalrService.spectatorGameStateReceived$.subscribe(
      state => {
        console.log('GameStateService: Received spectator state update via SignalR.');
        this.spectatorStateSignal.set(state);
      }
    );

    // Subscribe to player hand updates
    this.playerHandSubscription = this.signalrService.playerHandReceived$.subscribe(
      hand => {
        console.log('GameStateService: Received player hand update via SignalR.');
        this.playerHandSignal.set(hand);
      }
    );
  }

  /** Clears all current game state */
  public clearState(): void {
    this.spectatorStateSignal.set(null);
    this.playerHandSignal.set([]);
    this.isLoadingState.set(false);
    this.errorState.set(null);
  }

  /** Unsubscribes from all internal observables */
  private unsubscribeAll(): void {
    this.spectatorStateSubscription?.unsubscribe();
    this.playerHandSubscription?.unsubscribe();
  }

  /**
   * Connects to the Game Hub, fetches initial player state, and joins the game group.
   * Should be called when entering a game view.
   */
  public async connectToGame(gameId: string): Promise<void> {
    if (!this.signalrService.isBrowser) return; // Only run in browser

    const myPlayerId = this.authService.getCurrentPlayerId();
    if (!myPlayerId) {
      this.errorState.set("Cannot connect to game: User not identified.");
      console.error("GameStateService: Cannot connect to game, player ID not found.");
      return;
    }

    this.isLoadingState.set(true);
    this.errorState.set(null);
    this.clearState(); // Clear previous game state

    try {
      // 1. Fetch initial Player Game State via API
      console.log(`GameStateService: Fetching initial state for Game ${gameId}, Player ${myPlayerId}`);
      const initialState = await this.fetchInitialPlayerState(gameId, myPlayerId);

      // 2. Set initial state from API response
      this.spectatorStateSignal.set(initialState); // Set the full state initially
      this.playerHandSignal.set(initialState.playerHand ?? []); // Set the initial hand
      console.log("GameStateService: Initial state loaded.");

      // 3. Start SignalR connection (if not already started)
      if (this.signalrService.gameConnectionState() !== ConnectionState.Connected &&
        this.signalrService.gameConnectionState() !== ConnectionState.Connecting) {
        await this.signalrService.startGameConnection();
      }

      // 4. Join SignalR group if connected
      if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
        await this.signalrService.joinGameGroup(gameId);
      } else {
        throw new Error("Failed to establish SignalR connection."); // Throw if connection failed
      }

      // 5. Setup subscriptions for ongoing SignalR messages AFTER initial load
      this.setupSignalRSubscriptions();

    } catch (error: any) {
      console.error("GameStateService: Error connecting to game or fetching initial state:", error);
      this.errorState.set(error?.message ?? "Failed to load game state.");
      this.clearState(); // Clear partial state on error
    } finally {
      this.isLoadingState.set(false);
    }
  }

  /** Helper to fetch initial state with error handling */
  private fetchInitialPlayerState(gameId: string, playerId: string): Promise<PlayerGameStateDto> {
    return new Promise((resolve, reject) => {
      const sub = this.gameActionService.getPlayerState(gameId, playerId)
        .subscribe({
          next: (state) => {
            sub.unsubscribe(); // Manually unsubscribe after first value
            resolve(state);
          },
          error: (err) => {
            sub.unsubscribe(); // Manually unsubscribe on error
            reject(err);
          }
        });
    });
  }


  /**
   * Leaves the specified game group and potentially stops the connection.
   */
  public async disconnectFromGame(gameId: string): Promise<void> {
    this.unsubscribeAll(); // Unsubscribe from SignalR events
    if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
      await this.signalrService.leaveGameGroup(gameId);
    }
    // Optionally stop the connection if appropriate
    // await this.signalrService.stopGameConnection();
    this.clearState(); // Clear state when disconnecting
  }

  // Placeholder methods for stopping connections if needed by the effect
  private async stopNotificationConnection(): Promise<void> {
    await this.signalrService.stopNotificationConnection();
  }
  private async stopGameConnection(): Promise<void> {
    await this.signalrService.stopGameConnection();
  }

}

