import { Component, inject, computed, WritableSignal, signal, OnInit, OnDestroy, ChangeDetectionStrategy, Injector, Signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription, filter, take, firstValueFrom } from 'rxjs';

import { GameStateService } from '../services/game-state.service';
import { GameActionService } from '../services/game-action.service';
import { AuthService } from '@features/auth/services/auth.service';
import { SignalrService } from '@core/services/signalr.service';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { CARD_DETAILS_MAP } from '../components/card-display/CARD_DETAILS_MAP';

import { CardDisplayComponent } from '../components/card-display/card-display.component'; 
import { PlayerDisplayComponent } from '../components/player-display/player-display.component';
import { CurrentPlayerControlComponent } from '../components/current-player-control/current-player-control.component';

import { PlayerGameStateDto } from '../models/playerGameStateDto';
import { SpectatorGameStateDto } from '../models/spectatorGameStateDto';
import { CardDto } from '../models/cardDto';
import { PlayerHandInfoDto } from '../models/playerHandInfoDto';
import { SpectatorPlayerDto } from '../models/spectatorPlayerDto';
import { RoundEndSummaryDto } from '../models/roundEndSummaryDto';
import { PriestRevealData } from '../models/priestRevealData';
import { PlayCardRequestDto } from '../models/playCardRequestDto';

import { ActionModalComponent } from '../components/action-modal/action-modal.component';
import { ActionModalData } from '../actionModalData';
import { ActionModalResult } from '../actionModalResult';

import { RoundSummaryDialogComponent } from '../components/round-summary-dialog/round-summary-dialog.component';
import { GameOverDialogComponent, GameOverDialogData } from '../components/game-over-dialog.component';
import {PlayerStatus}from '@gameComponents/player-display/player.status';

interface NormalizedPlayer {
  id: string;
  name: string;
  cardCount: number;
  isProtected: boolean;
  isMe: boolean;
  status: number;
  tokensWon: number;
  playedCards: CardDto[];
  isCurrentTurn: boolean;
}

@Component({
  selector: 'app-game-view',
  templateUrl: './game-view.component.html',
  styleUrls: ['./game-view.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CardDisplayComponent,
    PlayerDisplayComponent,
    CurrentPlayerControlComponent,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTooltipModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GameViewComponent implements OnInit, OnDestroy {
  public readonly CARD_DETAILS_MAP = CARD_DETAILS_MAP; // Expose to template

  public gameStateService = inject(GameStateService);
  private gameActionService = inject(GameActionService);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private signalrService = inject(SignalrService);
  public uiInteractionService = inject(UiInteractionService);
  private injector = inject(Injector);

  // --- Expose GameStateService signals for template use ---
  public readonly isSpectating = this.gameStateService.isSpectating;
  public readonly gameState = this.gameStateService.gameState;
  // --------------------------------------------------------

  private gameIdSignal: WritableSignal<string | null> = signal(null);
  private currentPlayerId: string | null = null;

  public selectedCard: WritableSignal<CardDto | null> = signal(null);
  selectedTargetPlayerId: WritableSignal<string | null> = signal(null);
  guessedCardRankSignal: WritableSignal<number | null> = signal(null);

  public revealedCardSignal: WritableSignal<CardDto | null> = signal(null);
  public revealedCardTargetPlayerNameSignal: WritableSignal<string | null> = signal(null); // For Priest reveal message

  // Use the playerHand signal directly from GameStateService
  public myHandCards: Signal<CardDto[]> = this.gameStateService.playerHand;

  private priestRevealTimer: any = null;

  private subscriptions = new Subscription();

  gameId = this.gameStateService.gameId;
  isLoading = this.gameStateService.isLoading;
  error = this.gameStateService.error;
  isMyTurn = this.gameStateService.isMyTurn;
  gamePhase = this.gameStateService.gamePhase;
  currentTurnPlayerId = this.gameStateService.currentTurnPlayerId;

  // --- Raw Player DTOs for PlayerDisplayComponent ---
  allPlayersRaw = computed<(PlayerHandInfoDto | SpectatorPlayerDto)[]>(() => {
    const state = this.gameStateService.gameState();
    if (!state || !state.players) return [];
    // Assuming state.players are already correctly typed as (PlayerHandInfoDto | SpectatorPlayerDto)[]
    return state.players;
  });

  currentPlayerRaw = computed<(PlayerHandInfoDto | SpectatorPlayerDto) | undefined>(() => {
    const myId = this.currentPlayerId;
    if (!myId) return undefined;
    return this.allPlayersRaw().find(p => p.playerId === myId);
  });

  otherPlayersRaw = computed<(PlayerHandInfoDto | SpectatorPlayerDto)[]>(() => {
    const myId = this.currentPlayerId;
    if (!myId) return this.allPlayersRaw(); // If no current player ID, show all as others (e.g. spectator initial view)
    return this.allPlayersRaw().filter(p => p.playerId !== myId);
  });
  // -----------------------------------------------------

  // --- Re-derive normalized players from GameStateService.gameState --- // This section might be refactored or removed if PlayerDisplayComponent handles all display
  normalizedPlayersList = computed<NormalizedPlayer[]>(() => {
    const state = this.gameStateService.gameState();
    if (!state || !state.players) return [];

    const myId = this.currentPlayerId;
    const currentTurnPId = this.currentTurnPlayerId(); // Get current turn player ID
    return state.players.map((p: PlayerHandInfoDto | SpectatorPlayerDto) => {
      return {
        id: p.playerId,
        name: p.name,
        cardCount: p.handCardCount ?? 0,
        isProtected: p.isProtected || false,
        isMe: p.playerId === myId,
        status: p.status,
        tokensWon: p.tokensWon || 0,
        playedCards: p.playedCards || [],
        isCurrentTurn: p.playerId === currentTurnPId
      };
    });
  });
  // ----------------------------------------------------------------------

  // Public trackBy function for ngFor loops
  public trackPlayerById(index: number, player: NormalizedPlayer): string {
    return player.id;
  }

  public trackRawPlayerById(index: number, player: PlayerHandInfoDto | SpectatorPlayerDto): string {
    return player.playerId;
  }

  public currentPlayer = computed<NormalizedPlayer | undefined>(() => {
    return this.normalizedPlayersList().find(p => p.isMe);
  });

  public otherPlayers = computed<NormalizedPlayer[]>(() => {
    return this.normalizedPlayersList().filter(p => !p.isMe);
  });

  public onPlayerSelected(playerId: string): void {
    console.log('Player selected:', playerId);
    this.selectedTargetPlayerId.set(playerId);
    // Potentially trigger other actions or close a modal
  }

  public onCardGuessConfirmed(rank: number): void {
    console.log('Card guess confirmed:', rank);
    this.guessedCardRankSignal.set(rank);
    // Potentially trigger game action or close a modal
  }

  // --- Helper methods for PlayerDisplayComponent bindings ---
  isPlayerMe(player: PlayerHandInfoDto | SpectatorPlayerDto): boolean {
    if (!this.currentPlayerId) return false;
    return player.playerId === this.currentPlayerId;
  }

  isPlayerCurrentTurn(player: PlayerHandInfoDto | SpectatorPlayerDto): boolean {
    const turnPlayerId = this.currentTurnPlayerId();
    if (!turnPlayerId) return false;
    return player.playerId === turnPlayerId;
  }

  selectedCardRequiresTarget = computed<boolean>(() => {
    const card = this.selectedCard();
    if (!card) return false;
    return CARD_DETAILS_MAP[card.rank].requiresTarget ?? false;
  });

  canTargetRawPlayer = (player: PlayerHandInfoDto | SpectatorPlayerDto): boolean => {
    if (this.isSpectating() || !this.isMyTurn() || !this.selectedCardRequiresTarget()) return false;
    if (player.playerId === this.currentPlayerId) return false; // Cannot target self
    return player.status === PlayerStatus.Active && !player.isProtected;
  };
  // ---------------------------------------------------------

  targetablePlayers = computed<NormalizedPlayer[]>(() => {
    const myId = this.currentPlayerId;
    return this.normalizedPlayersList().filter(p => p.id !== myId && p.status === PlayerStatus.Active && !p.isProtected);
  });

  currentTurnPlayer = computed<NormalizedPlayer | undefined>(() => {
    const turnPlayerId = this.currentTurnPlayerId();
    if (!turnPlayerId) return undefined;
    return this.normalizedPlayersList().find(p => p.id === turnPlayerId);
  });

  // Computed signal to determine if the current player is active
  public isCurrentPlayerActive = computed<boolean>(() => {
    const player = this.currentPlayerRaw(); // Using currentPlayerRaw which is PlayerHandInfoDto | SpectatorPlayerDto
    return !!player && player.status === PlayerStatus.Active;
  });

  constructor() {
    effect(() => {
      const card = this.revealedCardSignal();
      if (!card && this.priestRevealTimer) {
        clearTimeout(this.priestRevealTimer);
        this.priestRevealTimer = null;
      }
    }, { injector: this.injector });
  }

  ngOnInit(): void {
    this.currentPlayerId = this.authService.getCurrentPlayerId();
    const gameIdFromRoute = this.route.snapshot.paramMap.get('id');

    if (gameIdFromRoute) {
      this.gameIdSignal.set(gameIdFromRoute);
      this.gameStateService.initializeGameConnection(gameIdFromRoute, this.currentPlayerId)
        .then(isSpectator => {
          console.log(`GameViewComponent: Initialized connection. Spectator mode: ${isSpectator}`);
          // Additional setup after connection can go here if needed
        })
        .catch(err => {
          console.error('GameViewComponent: Error initializing game connection:', err);
          // Error is already set in gameStateService, GameViewComponent template will display it.
        });
    } else {
      console.error('GameViewComponent: Game ID not found in route.');
      this.gameStateService.setError('Game ID not found. Please return to the lobby.');
    }

    this.subscriptions.add(
      this.gameStateService.roundWinnerAnnounced$.subscribe(summary => {
        if (summary) {
          this.openRoundSummaryDialog(summary);
        }
      })
    );

    this.subscriptions.add(
      this.gameStateService.gameWinnerAnnounced$.subscribe(event => {
        if (event && event.winnerId) {
          this.openGameOverDialog(event.winnerId);
        }
      })
    );

    // Subscription for Priest card reveal
    this.subscriptions.add(
      this.signalrService.priestRevealReceived$.subscribe((data: PriestRevealData) => {
        this.handlePriestReveal(data);
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    if (this.priestRevealTimer) {
      clearTimeout(this.priestRevealTimer);
    }
    // Corrected method name
    this.gameStateService.clearState(); // Reset state when leaving game view
    const gameId = this.gameIdSignal();
    if (gameId) {
      this.signalrService.leaveGameGroup(gameId);
    }
  }

  public clearPriestReveal(): void {
    this.revealedCardSignal.set(null);
    this.revealedCardTargetPlayerNameSignal.set(null);
    if (this.priestRevealTimer) {
      clearTimeout(this.priestRevealTimer);
      this.priestRevealTimer = null;
    }
  }

  private handlePriestReveal(data: PriestRevealData): void {
    this.revealedCardSignal.set(data.revealedCard);
    this.revealedCardTargetPlayerNameSignal.set(data.targetPlayerName);

    // Clear previous timer if any
    if (this.priestRevealTimer) {
      clearTimeout(this.priestRevealTimer);
    }

    // Set a new timer to hide the reveal after 5 seconds
    this.priestRevealTimer = setTimeout(() => {
      this.clearPriestReveal();
    }, 5000); // 5 seconds
  }

  onSelectCard(card: CardDto): void {
    if (!this.isMyTurn() || this.gameStateService.isSpectating()) return;

    const currentSelected = this.selectedCard();
    if (currentSelected && currentSelected.appearanceId === card.appearanceId) {
      this.selectedCard.set(null);
      this.selectedTargetPlayerId.set(null);
      this.guessedCardRankSignal.set(null);
    } else {
      this.selectedCard.set(card);
      this.selectedTargetPlayerId.set(null);
      this.guessedCardRankSignal.set(null);

      const cardDetail = CARD_DETAILS_MAP[card.rank];
      if (cardDetail && !cardDetail.requiresTarget && cardDetail.rank !== 1) {
      }
    }
  }

  selectTargetPlayer(playerId: string): void {
    if (!this.isMyTurn() || !this.selectedCard() || this.gameStateService.isSpectating()) return;
    this.selectedTargetPlayerId.set(playerId);

    const selectedCardRank = this.selectedCard()?.rank;
    if (selectedCardRank === 1) {
      this.promptForGuardGuess(playerId);
    } else {
      this.onSubmitMove();
    }
  }

  promptForGuardGuess(targetPlayerId: string): void {
    const dialogData: ActionModalData = {
      actionType: 'guess-card',
      prompt: `Guess a card (2-8) for player ${this.getPlayerName(targetPlayerId)}:`,
      availableCardTypes: Object.values(CARD_DETAILS_MAP)
        .filter(card => card.rank !== 1) // Exclude Guard
        .map(card => ({ value: card.rank, name: card.name })),
      excludeCardTypeValue: 1 // Exclude Guard (rank 1)
    };

    const dialogRef = this.dialog.open<ActionModalComponent, ActionModalData, ActionModalResult>(ActionModalComponent, {
      width: '350px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.selectedCardTypeValue !== undefined) {
        this.guessedCardRankSignal.set(result.selectedCardTypeValue);
        this.onSubmitMove(); // Proceed to submit move with the guess
      } else {
        // User cancelled or closed dialog without selection
        this.snackBar.open('Guard action cancelled.', 'Close', { duration: 2000 });
        this.selectedTargetPlayerId.set(null); // Reset target
        this.selectedCard.set(null); // Also reset selected card to avoid inconsistent state
        this.guessedCardRankSignal.set(null);
      }
    });
  }

  onSubmitMove(): void {
    const gameId = this.gameIdSignal();
    const sc = this.selectedCard();
    const targetId = this.selectedTargetPlayerId();
    const guessedRank = this.guessedCardRankSignal();

    if (!gameId || !sc || !this.isMyTurn()) {
      this.snackBar.open('Cannot play card: Not your turn or card not selected.', 'Close', { duration: 3000 });
      return;
    }

    const cardDetail = CARD_DETAILS_MAP[sc.rank];
    if (!cardDetail) {
      console.error('Card details not found for rank:', sc.rank);
      this.snackBar.open('Error: Card details not found.', 'Close', { duration: 3000 });
      return;
    }

    if (cardDetail.requiresTarget && !targetId && sc.rank !== 8) {
      const otherTargetablePlayers = this.targetablePlayers();
      if (sc.rank === 3 || sc.rank === 2 || sc.rank === 6) {
        if (otherTargetablePlayers.length === 0) {
        } else {
          this.snackBar.open(`Please select a target player for ${cardDetail.name}.`, 'Close', { duration: 3000 });
          return;
        }
      } else {
        this.snackBar.open(`Please select a target player for ${cardDetail.name}.`, 'Close', { duration: 3000 });
        return;
      }
    }

    if (sc.rank === 1 && (guessedRank === null || guessedRank < 2 || guessedRank > 8)) {
      this.snackBar.open('Please make a valid guess (2-8) for the Guard.', 'Close', { duration: 3000 });
      return;
    }

    this.gameStateService.setLoading(true);
    firstValueFrom(this.gameActionService.playCard(gameId, sc.appearanceId, targetId, guessedRank))
      .then(() => {
        this.snackBar.open(`Played ${cardDetail.name}.`, 'Close', { duration: 2000 });
      })
      .catch((error: any) => {
        console.error('Error playing card:', error);
        this.gameStateService.setError(error.message || 'Failed to play card.');
      })
      .finally(() => {
        this.gameStateService.setLoading(false);
        this.resetTurnState();
      });
  }

  private resetTurnState(): void {
    this.selectedCard.set(null);
    this.selectedTargetPlayerId.set(null);
    this.guessedCardRankSignal.set(null);
  }

  getPlayerTokens(playerId: string): number {
    const state = this.gameStateService.gameState();
    if (state && state.players) {
      const playerInfo = state.players.find((p: any) => p.playerId === playerId);
      return playerInfo?.tokensWon || 0;
    }
    return 0;
  }

  canTargetPlayer(player: NormalizedPlayer): boolean {
    if (!this.isMyTurn() || !this.selectedCard() || this.gameStateService.isSpectating()) {
      return false;
    }
    return !!this.targetablePlayers().find(tp => tp.id === player.id);
  }

  getPlayerName(playerId: string): string {
    const player = this.normalizedPlayersList().find(p => p.id === playerId);
    return player ? player.name : 'Unknown Player';
  }

  public canPlaySelectedCard(): boolean {
    const selectedCard = this.selectedCard();
    if (!selectedCard || !this.isMyTurn()) {
      return false;
    }

    const cardDetails = this.CARD_DETAILS_MAP[selectedCard.rank];
    const hand = this.myHandCards();

    // Countess logic: Must play Countess if holding King or Prince
    const hasCountess = hand.some(c => c.rank === 7);
    const hasKingOrPrince = hand.some(c => c.rank === 5 || c.rank === 6);
    if (hasCountess && hasKingOrPrince && selectedCard.rank !== 7) {
      return false; // If holding Countess and King/Prince, only Countess is playable
    }

    if (cardDetails.requiresTarget) {
      if (!this.selectedTargetPlayerId()) {
        return false; // Target required but not selected
      }
      // Additional check for Guard: requires target AND a guess (handled by modal)
      // For now, if target is required and selected, consider it playable from this method's perspective.
      // The modal pop-up for Guard will be handled in onPlayCard.
    }
    return true;
  }

  public async onPlayCard(): Promise<void> {
    if (!this.isMyTurn() || !this.selectedCard() || !this.canPlaySelectedCard()) {
      this.snackBar.open('Cannot play card now.', 'Dismiss', { duration: 2000 });
      return;
    }

    const gameId = this.gameIdSignal();
    const cardToPlay = this.selectedCard();
    let targetPlayerId = this.selectedTargetPlayerId();
    let guessedCardRank = this.guessedCardRankSignal();

    if (!gameId || !cardToPlay) {
      console.error('Game ID or card to play is missing.');
      this.gameStateService.setError('Could not play card: Game ID or card missing.');
      return;
    }

    const cardDetails = this.CARD_DETAILS_MAP[cardToPlay.rank];

    // Handle Guard (rank 1) guess
    if (cardToPlay.rank === 1) {
      if (!targetPlayerId) {
        this.snackBar.open('Please select a target player for the Guard.', 'Dismiss', { duration: 3000 });
        return;
      }
      const targetPlayer = this.normalizedPlayersList().find(p => p.id === targetPlayerId);
      if (!targetPlayer) return;

      const dialogRef = this.dialog.open<ActionModalComponent, ActionModalData, ActionModalResult>(
        ActionModalComponent,
        {
          data: {
            actionType: 'guess-card',
            prompt: `Guess a card (not Guard) for ${targetPlayer.name}:`,
            availableCardTypes: Object.entries(this.CARD_DETAILS_MAP)
              .filter(([rankStr, _]) => parseInt(rankStr, 10) !== 1) // Exclude Guard
              .map(([rankStr, details]) => ({ value: parseInt(rankStr, 10), name: details.name })),
            excludeCardTypeValue: 1, // Guard
          },
        }
      );

      const result = await firstValueFrom(dialogRef.afterClosed());
      if (result && result.selectedCardTypeValue) {
        guessedCardRank = result.selectedCardTypeValue;
      } else {
        return; // User cancelled or no guess made
      }
    }

    // For cards like Prince (5) that can target self
    if (cardToPlay.rank === 5 && !targetPlayerId) {
      targetPlayerId = this.currentPlayerId;
    }

    if (cardDetails.requiresTarget && !targetPlayerId && cardToPlay.rank !== 5) { // Prince can target self implicitly
        this.snackBar.open(`The ${cardDetails.name} requires a target player.`, 'Dismiss', { duration: 3000 });
        return;
    }

    this.gameStateService.setLoading(true);
    try {
      await firstValueFrom(
        this.gameActionService.playCard(gameId, cardToPlay.appearanceId, targetPlayerId, guessedCardRank)
      );
      this.selectedCard.set(null);
      this.selectedTargetPlayerId.set(null);
      this.guessedCardRankSignal.set(null);
    } catch (error: any) {
      console.error('Error playing card:', error);
      const errorMessage = error?.error?.Message || error?.message || 'Failed to play card.';
      this.gameStateService.setError(errorMessage);
      this.snackBar.open(errorMessage, 'Dismiss', { duration: 3000 });
    } finally {
      this.gameStateService.setLoading(false);
    }
  }

  public handleHandCardClicked(card: CardDto): void {
    this.onSelectCard(card); // Corrected method call
  }

  public handlePlayCardClicked(): void {
    this.onPlayCard(); // Corrected method call
  }

  openRoundSummaryDialog(summaryData: RoundEndSummaryDto): void {
    this.dialog.open(RoundSummaryDialogComponent, {
      data: summaryData,
      width: '500px',
      disableClose: true
    });
  }

  openGameOverDialog(winnerId: string): void {
    const winnerPlayer = this.normalizedPlayersList().find(p => p.id === winnerId);
    const winnerName = winnerPlayer ? winnerPlayer.name : `Player ${winnerId.substring(0, 6)}`;

    this.dialog.open(GameOverDialogComponent, {
      data: { winnerName: winnerName, winnerId: winnerId } as GameOverDialogData,
      width: '400px',
      disableClose: true
    });
  }
}
