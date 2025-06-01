import { Injectable, Inject, PLATFORM_ID, signal, WritableSignal, Signal, OnDestroy, effect, Injector } from '@angular/core'; // Import signal utilities, PLATFORM_ID, Inject
import { isPlatformBrowser } from '@angular/common';
import * as signalR from '@microsoft/signalr'; // Import SignalR client library
import { Subject } from 'rxjs'; // Removed BehaviorSubject import

import { AuthService } from './auth.service';
import {SpectatorGameStateDto} from '../models/spectatorGameStateDto';
import {CardDto} from '../models/cardDto';
import {RoundEndSummaryDto} from '../models/roundEndSummaryDto';
import { PriestRevealData } from '../models/priestRevealData'; // Import PriestRevealData

// Define connection states
export enum ConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Disconnecting = 'Disconnecting',
  Reconnecting = 'Reconnecting'
}

@Injectable({
  providedIn: 'root'
})
export class SignalrService implements OnDestroy {
  // --- Make isBrowser public readonly ---
  public readonly isBrowser: boolean;
  private notificationHubConnection?: signalR.HubConnection;
  private gameHubConnection?: signalR.HubConnection;

  // --- Connection State Signals ---
  // Use WritableSignal for internal state management
  private notificationConnectionStateSignal: WritableSignal<ConnectionState>;
  private gameConnectionStateSignal: WritableSignal<ConnectionState>;

  // Expose readonly signals for external consumption
  public notificationConnectionState: Signal<ConnectionState>;
  public gameConnectionState: Signal<ConnectionState>;
  // --- End Connection State Signals ---

  // --- Subjects for Server-to-Client Messages (Keep as Subjects/Observables for event streams) ---
  private gameInvitationSubject = new Subject<{ gameId: string, creatorName: string }>();
  public gameInvitationReceived$ = this.gameInvitationSubject.asObservable();
  private spectatorGameStateSubject = new Subject<SpectatorGameStateDto>();
  public spectatorGameStateReceived$ = this.spectatorGameStateSubject.asObservable();
  private playerHandSubject = new Subject<CardDto[]>();
  public playerHandReceived$ = this.playerHandSubject.asObservable();
  private roundSummarySubject = new Subject<RoundEndSummaryDto>(); // Changed name and type
  public roundSummaryReceived$ = this.roundSummarySubject.asObservable();
  private gameWinnerSubject = new Subject<{ winnerId: string }>();
  public gameWinnerReceived$ = this.gameWinnerSubject.asObservable();
  private priestRevealSubject = new Subject<PriestRevealData>();
  public priestRevealReceived$ = this.priestRevealSubject.asObservable();

  constructor(
    @Inject(PLATFORM_ID) private platformId: object,
    private authService: AuthService, // Inject AuthService
    private injector: Injector // Inject Injector for effect context
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);

    // Initialize signals
    this.notificationConnectionStateSignal = signal<ConnectionState>(ConnectionState.Disconnected);
    this.gameConnectionStateSignal = signal<ConnectionState>(ConnectionState.Disconnected);
    this.notificationConnectionState = this.notificationConnectionStateSignal.asReadonly();
    this.gameConnectionState = this.gameConnectionStateSignal.asReadonly();


    // --- Use effect to react to isLoggedIn signal changes ---
    effect(() => {
      const isLoggedIn = this.authService.isLoggedIn(); // Read the signal value
      if (!isLoggedIn && this.isBrowser) { // Only react when false and in browser
        console.log('User logged out (detected by signal), stopping SignalR connections.');
        Promise.all([
          this.stopNotificationConnection(),
          this.stopGameConnection()
        ]).catch(err => console.error("Error stopping SignalR connections on logout:", err));
      }
    }, { injector: this.injector }); // Provide injector context for the effect
    // --- End effect usage ---

  }

  ngOnDestroy(): void {
    // Clean up connections when service is destroyed
    this.stopNotificationConnection();
    this.stopGameConnection();
  }

  // --- Notification Hub Methods ---

  public async startNotificationConnection(): Promise<void> {
    // Read signal value using ()
    if (!this.isBrowser || this.notificationConnectionState() === ConnectionState.Connected || this.notificationConnectionState() === ConnectionState.Connecting) {
      return;
    }
    if (!this.authService.isLoggedIn()) {
      console.log('User not logged in, skipping NotificationHub connection.');
      return;
    }

    this.notificationConnectionStateSignal.set(ConnectionState.Connecting); // Use .set()
    console.log('Starting NotificationHub connection...');

    this.notificationHubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notification')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Register handlers...
    this.notificationHubConnection.on('ReceiveGameInvitation', (gameId: string, creatorName: string) => {
      console.log(`Received game invitation: GameId=${gameId}, Creator=${creatorName}`);
      this.gameInvitationSubject.next({ gameId, creatorName });
    });

    // Handle connection lifecycle events using .set()
    this.notificationHubConnection.onreconnecting((error?: Error) => {
      console.warn(`NotificationHub connection reconnecting due to: ${error?.message ?? 'Unknown reason'}`);
      this.notificationConnectionStateSignal.set(ConnectionState.Reconnecting);
    });

    this.notificationHubConnection.onreconnected((connectionId?: string) => {
      console.log(`NotificationHub connection reconnected with ID: ${connectionId ?? 'N/A'}`);
      this.notificationConnectionStateSignal.set(ConnectionState.Connected);
    });

    this.notificationHubConnection.onclose((error?: Error) => {
      console.warn(`NotificationHub connection closed due to: ${error?.message ?? 'Unknown reason'}`);
      this.notificationConnectionStateSignal.set(ConnectionState.Disconnected);
    });

    try {
      await this.notificationHubConnection.start();
      this.notificationConnectionStateSignal.set(ConnectionState.Connected); // Use .set()
      console.log('NotificationHub connection established.');
    } catch (err) {
      console.error('Error starting NotificationHub connection:', err);
      this.notificationConnectionStateSignal.set(ConnectionState.Disconnected); // Use .set()
    }
  }

  public async stopNotificationConnection(): Promise<void> {
    // Read signal value using ()
    if (this.notificationHubConnection && this.notificationConnectionState() !== ConnectionState.Disconnected && this.notificationConnectionState() !== ConnectionState.Disconnecting) {
      this.notificationConnectionStateSignal.set(ConnectionState.Disconnecting); // Use .set()
      console.log('Stopping NotificationHub connection...');
      try {
        await this.notificationHubConnection.stop();
        console.log('NotificationHub connection stopped.');
      } catch (err) {
        console.error('Error stopping NotificationHub connection:', err);
      } finally {
        this.notificationConnectionStateSignal.set(ConnectionState.Disconnected); // Use .set()
      }
    }
  }

  // --- Game Hub Methods ---

  public async startGameConnection(): Promise<void> {
    // Read signal value using ()
    if (!this.isBrowser || this.gameConnectionState() === ConnectionState.Connected || this.gameConnectionState() === ConnectionState.Connecting) {
      return;
    }

    this.gameConnectionStateSignal.set(ConnectionState.Connecting); // Use .set()
    console.log('Starting GameHub connection...');

    this.gameHubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/game')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Register handlers... (using .next() on Subjects is still correct here)
    this.gameHubConnection.on('UpdateSpectatorGameState', (gameState: SpectatorGameStateDto) => this.spectatorGameStateSubject.next(gameState));
    this.gameHubConnection.on('UpdatePlayerHand', (currentHand: CardDto[]) => this.playerHandSubject.next(currentHand));
    this.gameHubConnection.on('ShowRoundSummary', (summaryData: RoundEndSummaryDto) => { // Matches IGameClient method name
      console.log('Received Round Summary (ShowRoundSummary):', summaryData);
      this.roundSummarySubject.next(summaryData); // Emit the full DTO
    });
    this.gameHubConnection.on('GameWinnerAnnounced', (winnerId: string) => this.gameWinnerSubject.next({ winnerId }));

    // Handler for Priest card reveal
    this.gameHubConnection.on('ReceivePriestReveal',
      (targetPlayerId: string, targetPlayerName: string, revealedCard: CardDto) => {
        console.log('Priest Reveal Received:', { targetPlayerId, targetPlayerName, revealedCard });
        this.priestRevealSubject.next({ targetPlayerId, targetPlayerName, revealedCard });
    });

    // Handle connection lifecycle events using .set()
    this.gameHubConnection.onreconnecting((error?: Error) => {
      console.warn(`GameHub connection reconnecting due to: ${error?.message ?? 'Unknown reason'}`);
      this.gameConnectionStateSignal.set(ConnectionState.Reconnecting);
    });

    this.gameHubConnection.onreconnected((connectionId?: string) => {
      console.log(`GameHub connection reconnected with ID: ${connectionId ?? 'N/A'}`);
      this.gameConnectionStateSignal.set(ConnectionState.Connected);
      // IMPORTANT: Client might need to re-join game groups after reconnecting
    });

    this.gameHubConnection.onclose((error?: Error) => {
      console.warn(`GameHub connection closed due to: ${error?.message ?? 'Unknown reason'}`);
      this.gameConnectionStateSignal.set(ConnectionState.Disconnected);
    });

    try {
      await this.gameHubConnection.start();
      this.gameConnectionStateSignal.set(ConnectionState.Connected); // Use .set()
      console.log('GameHub connection established.');
    } catch (err) {
      console.error('Error starting GameHub connection:', err);
      this.gameConnectionStateSignal.set(ConnectionState.Disconnected); // Use .set()
    }
  }

  public async stopGameConnection(): Promise<void> {
    // Read signal value using ()
    if (this.gameHubConnection && this.gameConnectionState() !== ConnectionState.Disconnected && this.gameConnectionState() !== ConnectionState.Disconnecting) {
      this.gameConnectionStateSignal.set(ConnectionState.Disconnecting); // Use .set()
      console.log('Stopping GameHub connection...');
      try {
        await this.gameHubConnection.stop();
        console.log('GameHub connection stopped.');
      } catch (err) {
        console.error('Error stopping GameHub connection:', err);
      } finally {
        this.gameConnectionStateSignal.set(ConnectionState.Disconnected); // Use .set()
      }
    }
  }

  // --- Client-to-Server Invocation Methods ---

  public async joinGameGroup(gameId: string): Promise<void> {
    // Read signal value using ()
    if (this.gameHubConnection?.state !== signalR.HubConnectionState.Connected) {
      console.warn('Cannot join game group, GameHub not connected.');
      return;
    }
    try {
      await this.gameHubConnection.invoke('JoinGameGroup', gameId);
      console.log(`Requested to join game group for GameId: ${gameId}`);
    } catch (err) {
      console.error(`Error invoking JoinGameGroup for GameId ${gameId}:`, err);
    }
  }

  public async leaveGameGroup(gameId: string): Promise<void> {
    // Read signal value using ()
    if (this.gameHubConnection?.state !== signalR.HubConnectionState.Connected) {
      console.warn('Cannot leave game group, GameHub not connected.');
      return;
    }
    try {
      await this.gameHubConnection.invoke('LeaveGameGroup', gameId);
      console.log(`Requested to leave game group for GameId: ${gameId}`);
    } catch (err) {
      console.error(`Error invoking LeaveGameGroup for GameId ${gameId}:`, err);
    }
  }

  // Add other invoke methods here if needed...

}
