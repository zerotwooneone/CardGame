import {Injectable, signal, WritableSignal, Signal, OnDestroy, effect, Injector, computed, inject} from '@angular/core';
import {Observable, Subject, Subscription, firstValueFrom } from 'rxjs';
import { SignalrService, ConnectionState } from '@core/services/signalr.service';
import { AuthService } from '@features/auth/services/auth.service';
import {SpectatorGameStateDto} from '../models/spectatorGameStateDto';
import {CardDto} from '../models/cardDto';
import {GameActionService} from './game-action.service';
import {PlayerGameStateDto} from '../models/playerGameStateDto';
import {RoundEndSummaryDto} from '../models/roundEndSummaryDto';
import { GameLogEntryDto } from '../models/gameLogEntryDto'; 
import { DeckService } from './deck.service'; 
import { GamePhase } from '../models/gamePhase'; 


@Injectable({
  // Provide locally within the game feature or root if needed elsewhere
  providedIn: 'root'
})
export class GameStateService implements OnDestroy {

  // --- Private Writable Signals holding the state ---
  // Renamed and re-typed to hold either player or spectator state
  private combinedStateSignal: WritableSignal<PlayerGameStateDto | SpectatorGameStateDto | null> = signal(null);
  private playerHandSignal: WritableSignal<CardDto[]> = signal([]);
  private isLoadingState: WritableSignal<boolean> = signal(false);
  private errorState: WritableSignal<string | null> = signal(null);

  // --- Public Readonly Signals for consumption ---
  public isSpectating: WritableSignal<boolean> = signal(false); // New signal for spectator mode
  // Renamed to reflect its combined nature
  public gameState: Signal<PlayerGameStateDto | SpectatorGameStateDto | null> = this.combinedStateSignal.asReadonly();
  public playerHand: Signal<CardDto[]> = this.playerHandSignal.asReadonly();
  public isLoading: Signal<boolean> = this.isLoadingState.asReadonly();
  public error: Signal<string | null> = this.errorState.asReadonly();
  public gameLogs: Signal<GameLogEntryDto[]> = computed(() => {
    const state = this.combinedStateSignal();
    if (state && state.gameLog && Array.isArray(state.gameLog)) {
      return state.gameLog;
    }
    return [];
  });

  // --- Computed Signals for derived state ---
  // Updated to use combinedStateSignal
  public gameId: Signal<string | null> = computed(() => this.combinedStateSignal()?.gameId ?? null);
  public currentTurnPlayerId: Signal<string | null> = computed(() => this.combinedStateSignal()?.currentTurnPlayerId ?? null);
  public isMyTurn: Signal<boolean> = computed(() => {
    const myId = this.authService.getCurrentPlayerId();
    const currentTurnId = this.currentTurnPlayerId(); // Uses the updated computed signal above
    return !!myId && !!currentTurnId && myId === currentTurnId;
  });
  public gamePhase: Signal<string | null> = computed(() => {
    const state = this.combinedStateSignal();
    if (!state || state.gamePhase === null || state.gamePhase === undefined) return null;

    switch (state.gamePhase as GamePhase) {
      case GamePhase.NotStarted: return 'NotStarted';
      case GamePhase.RoundInProgress: return 'RoundInProgress';
      case GamePhase.RoundOver: return 'RoundOver';
      case GamePhase.GameOver: return 'GameOver';
      default: return null; // Or handle unknown number
    }
  });

  // --- Observables for transient events ---
  // Declare properties first
  public readonly roundWinnerAnnounced$: Observable<RoundEndSummaryDto>;
  public readonly gameWinnerAnnounced$: Observable<{ winnerId: string }>;

  // --- Subscriptions ---
  private spectatorStateSubscription?: Subscription;
  private playerHandSubscription?: Subscription;

  // Inject services using inject() for consistency or keep constructor injection
  private signalrService = inject(SignalrService);
  private authService = inject(AuthService);
  private gameActionService = inject(GameActionService); // Inject GameActionService
  private deckService = inject(DeckService); // Inject DeckService
  private injector = inject(Injector);

  constructor() {
    // Assign Observables in constructor
    // This ensures this.signalrService is initialized before being accessed
    // Correctly assigns the observable from SignalrService
    this.roundWinnerAnnounced$ = this.signalrService.roundSummaryReceived$; // Previously roundWinnerReceived$
    this.gameWinnerAnnounced$ = this.signalrService.gameWinnerReceived$;

    this.setupEffects();
  }

  ngOnDestroy(): void {
    this.unsubscribeAll();
    // Effects tied to the injector are automatically cleaned up
    const currentId = this.gameId();
    if (currentId) {
      // Ensure disconnect is also safe for spectator vs player if different logic needed
      this.signalrService.leaveGameGroup(currentId);
      // No need to stop game connection here, as it might be shared or handled by SignalR service itself on app close/logout
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
      state => { // state here is SpectatorGameStateDto
        console.log(`GameStateService Received spectator state update via SignalR.`);
        this.combinedStateSignal.set(state); // Update combinedStateSignal
        if (state?.deckDefinitionId) {
          this.deckService.setSelectedDeckId(state.deckDefinitionId);
        }
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

  // --- Public Methods to Modify State ---
  public setLoading(value: boolean): void {
    this.isLoadingState.set(value);
  }

  public setError(message: string | null): void {
    this.errorState.set(message);
  }

  /** Clears all current game state */
  public clearState(): void {
    this.combinedStateSignal.set(null); // Update to clear combinedStateSignal
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

    this.errorState.set(null); // Still clear specific errors for this operation
    this.isSpectating.set(false); // Default to not spectating

    try {
      // 1. Fetch initial Player Game State via API
      console.log(`GameStateService: Fetching initial state for Game ${gameId}, Player ${myPlayerId}`);
      const initialState = await this.fetchInitialPlayerState(gameId, myPlayerId); // initialState is PlayerGameStateDto

      // 2. Set initial state from API response
      this.combinedStateSignal.set(initialState); // Set combinedStateSignal with PlayerGameStateDto
      if (initialState?.deckDefinitionId) {
        this.deckService.setSelectedDeckId(initialState.deckDefinitionId);
      }
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
    }
  }

  /**
   * Initializes the connection to a game, determining whether to load
   * player-specific state or spectator state.
   * @param gameId The ID of the game to connect to.
   * @param currentUserId The ID of the current user, if logged in. Null otherwise.
   * @returns Promise<boolean> True if in spectator mode, false otherwise.
   */
  public async initializeGameConnection(gameId: string, currentUserId: string | null): Promise<boolean> {
    if (!this.signalrService.isBrowser) return true; // Default to spectating SSR/Pre-render

    this.isLoadingState.set(true);
    this.errorState.set(null);
    this.clearState();
    let isSpectator = true; // Assume spectator mode initially

    try {
      if (currentUserId) {
        console.log(`GameStateService: Attempting to fetch player state for Game ${gameId}, Player ${currentUserId}`);
        try {
          const playerState = await this.fetchInitialPlayerState(gameId, currentUserId);
          this.combinedStateSignal.set(playerState);
          if (playerState?.deckDefinitionId) {
            this.deckService.setSelectedDeckId(playerState.deckDefinitionId);
          }
          this.playerHandSignal.set(playerState.playerHand ?? []);
          console.log("GameStateService: Player state loaded.");
          isSpectator = false;
        } catch (playerStateError: any) {
          console.warn(`GameStateService: Could not fetch player state (User: ${currentUserId}, Game: ${gameId}). Error: ${playerStateError?.message}. Falling back to spectator mode.`);
          // If player state fails (e.g., not a player in this game), we fall through to spectator mode.
        }
      }

      if (isSpectator) {
        console.log(`GameStateService: Fetching spectator state for Game ${gameId}`);
        const spectatorState = await this.fetchInitialSpectatorState(gameId);
        this.combinedStateSignal.set(spectatorState);
        if (spectatorState?.deckDefinitionId) {
          this.deckService.setSelectedDeckId(spectatorState.deckDefinitionId);
        }
        this.playerHandSignal.set([]); // Ensure hand is empty for spectators
        console.log("GameStateService: Spectator state loaded.");
      }

      this.isSpectating.set(isSpectator);

      // Common SignalR setup for both players and spectators
      if (this.signalrService.gameConnectionState() !== ConnectionState.Connected &&
          this.signalrService.gameConnectionState() !== ConnectionState.Connecting) {
        await this.signalrService.startGameConnection();
      }

      if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
        await this.signalrService.joinGameGroup(gameId);
      } else {
        throw new Error("Failed to establish SignalR connection for game updates.");
      }
      this.setupSignalRSubscriptions();
      this.isLoadingState.set(false);
      return isSpectator;

    } catch (error: any) {
      console.error(`GameStateService: Error initializing game connection for ${gameId}:`, error);
      this.errorState.set(error?.message || 'Failed to connect to the game.');
      this.isLoadingState.set(false);
      this.isSpectating.set(true); // Ensure spectator mode on error
      throw error; // Re-throw for the component to handle if needed
    }
  }

  /**
   * Joins a game as a spectator.
   * @param gameId The ID of the game to join as a spectator.
   */
  public async joinAsSpectator(gameId: string): Promise<void> {
    if (!this.signalrService.isBrowser) return;

    // isLoadingState is managed by the calling method (e.g., initializeGameConnection)
    this.errorState.set(null); // Still clear specific errors for this operation
    // this.clearState(); // Clear previous game state - Handled by initializeGameConnection
    this.isSpectating.set(true);

    try {
      console.log(`GameStateService: Fetching spectator state for Game ${gameId}`);
      const spectatorState = await this.fetchInitialSpectatorState(gameId);
      this.combinedStateSignal.set(spectatorState);
      if (spectatorState?.deckDefinitionId) {
        this.deckService.setSelectedDeckId(spectatorState.deckDefinitionId);
      }
      this.playerHandSignal.set([]); // Ensure hand is empty for spectators
      console.log("GameStateService: Spectator state loaded.");

      // Common SignalR setup for both players and spectators
      if (this.signalrService.gameConnectionState() !== ConnectionState.Connected &&
          this.signalrService.gameConnectionState() !== ConnectionState.Connecting) {
        await this.signalrService.startGameConnection();
      }

      if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
        await this.signalrService.joinGameGroup(gameId);
      } else {
        throw new Error("Failed to establish SignalR connection for game updates.");
      }
      this.setupSignalRSubscriptions(); // Ensure subscriptions are (re)established
      console.log(`GameStateService: Successfully joined game ${gameId} as spectator.`);
    } catch (e) {
      console.error(`GameStateService: Error joining game ${gameId} as spectator:`, e);
      this.errorState.set(`Failed to join game as spectator. ${e instanceof Error ? e.message : String(e)}`);
    } finally {
      // isLoadingState is managed by the calling method (e.g., initializeGameConnection)
    }
  }

  private async fetchInitialSpectatorState(gameId: string): Promise<SpectatorGameStateDto> {
    this.isLoadingState.set(true);
    try {
      // Using existing getGameState and casting, assuming it can return SpectatorGameStateDto if no playerId
      const spectatorStateData = await firstValueFrom(this.gameActionService.getSpectatorGameState(gameId)); // Correctly await
      if (!spectatorStateData || typeof (spectatorStateData as SpectatorGameStateDto).gamePhase === 'undefined') { // Basic validation on resolved data
          throw new Error('Invalid spectator state received from server.');
      }
      this.isLoadingState.set(false);
      return spectatorStateData as SpectatorGameStateDto; // Cast if necessary, ensure type safety
    } catch (error) {
      console.error(`GameStateService: Error fetching initial spectator state for game ${gameId}:`, error);
      this.errorState.set(`Failed to load game (spectator). Please try again. Error: ${(error as Error).message}`);
      this.isLoadingState.set(false);
      throw error; // Rethrow to be caught by calling function
    }
  }

  public async connectToSpectateGame(gameId: string): Promise<void> {
    if (!this.signalrService.isBrowser) return;

    this.errorState.set(null);
    this.isSpectating.set(true); // Set spectator mode to true
    this.isLoadingState.set(true);

    try {
      console.log(`GameStateService: Fetching initial spectator state for Game ${gameId}`);
      const initialState = await this.fetchInitialSpectatorState(gameId);

      this.combinedStateSignal.set(initialState);
      if (initialState?.deckDefinitionId) {
        this.deckService.setSelectedDeckId(initialState.deckDefinitionId);
      }
      // Spectators don't have a hand, but clear it to be safe
      this.playerHandSignal.set([]);
      console.log("GameStateService: Initial spectator state loaded.");

      if (this.signalrService.gameConnectionState() !== ConnectionState.Connected &&
          this.signalrService.gameConnectionState() !== ConnectionState.Connecting) {
        await this.signalrService.startGameConnection();
      }

      if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
        await this.signalrService.joinGameGroup(gameId);
      } else {
        throw new Error("Failed to establish SignalR connection for spectating.");
      }

      this.setupSignalRSubscriptions();
      this.isLoadingState.set(false);
      console.log(`GameStateService: Successfully connected spectator to game ${gameId}.`);

    } catch (error) {
      console.error(`GameStateService: Error connecting to spectate game ${gameId}:`, error);
      // Error already set by fetchInitialSpectatorState or other specific errors
      // this.errorState.set(`Failed to connect to game as spectator. Error: ${(error as Error).message}`);
      this.isLoadingState.set(false);
      // No need to clear state here as it might be useful for debugging or partial loads
    }
  }

  /** Helper to fetch initial state with error handling */
  private fetchInitialPlayerState(gameId: string, playerId: string): Promise<PlayerGameStateDto> {
    return new Promise((resolve, reject) => {
      const sub = this.gameActionService.getPlayerState(gameId, playerId)
        .subscribe({
          next: (state) => { // state here is PlayerGameStateDto
            if (!state) {
              reject(new Error("Initial game state not received or is null."));
              return;
            }
            // No explicit mapping needed here, combinedStateSignal handles PlayerGameStateDto directly
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
   * Disconnects from the current game's SignalR group.
   */
  public disconnectFromGame(gameId: string): void {
    this.unsubscribeAll(); // Unsubscribe from SignalR events
    if (this.signalrService.gameConnectionState() === ConnectionState.Connected) {
      this.signalrService.leaveGameGroup(gameId);
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
