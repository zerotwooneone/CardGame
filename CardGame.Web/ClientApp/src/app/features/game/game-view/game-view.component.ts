import { Component, OnInit, OnDestroy, inject, signal, WritableSignal, computed, Signal, ChangeDetectionStrategy, ChangeDetectorRef, effect, Injector } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { GameStateService } from '../services/game-state.service';
import { GameActionService } from '../services/game-action.service';
import { AuthService } from '../../../core/services/auth.service';
import { PlayerDisplayComponent } from '../components/player-display/player-display.component';
import { CardComponent } from '../components/card/card.component';
import { ActionModalComponent } from '../components/action-modal/action-modal.component';
import {SpectatorGameStateDto} from '../../../core/models/spectatorGameStateDto';
import { CardDto } from '../../../core/models/cardDto';
import {ActionModalData} from '../actionModalData';
import {ActionModalResult} from '../actionModalResult';
import {PlayCardRequestDto} from '../../../core/models/playCardRequestDto';
import {SpectatorPlayerDto} from '../../../core/models/spectatorPlayerDto';
import {CARD_DETAILS_MAP} from '../components/card/CARD_DETAILS_MAP';
import {UiInteractionService} from '../../../core/services/ui-interaction-service.service';
import {RoundEndSummaryDto} from '../../../core/models/roundEndSummaryDto';
import {PlayerHandInfoDto} from '../../../core/models/playerHandInfoDto';
import {RoundSummaryDialogComponent} from '../components/round-summary-dialog/round-summary-dialog.component';
import {PlayerGameStateDto} from '../../../core/models/playerGameStateDto';

const getCardNameFromValue = (value: number | undefined): string => {
  if (value === undefined) return '?';
  return CARD_DETAILS_MAP[value]?.name ?? '?';
};

@Component({
  selector: 'app-game-view',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatSnackBarModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    PlayerDisplayComponent,
    CardComponent
  ],
  templateUrl: './game-view.component.html',
  styleUrls: ['./game-view.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GameViewComponent implements OnInit, OnDestroy {
  // Inject services
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private gameStateService = inject(GameStateService);
  private gameActionService = inject(GameActionService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private cdr = inject(ChangeDetectorRef);
  private uiInteractionService = inject(UiInteractionService);

  private destroy$ = new Subject<void>();

  // Game State Signals from Service (public readonly)
  spectatorState: Signal<PlayerGameStateDto | SpectatorGameStateDto | null> = this.gameStateService.gameState;
  playerHand: Signal<CardDto[]> = this.gameStateService.playerHand;
  isMyTurn: Signal<boolean> = this.gameStateService.isMyTurn;
  gamePhase: Signal<string | null> = this.gameStateService.gamePhase;
  gameId: Signal<string | null> = this.gameStateService.gameId;
  isSpectating: Signal<boolean> = this.gameStateService.isSpectating;
  isLoadingFromService: Signal<boolean> = this.gameStateService.isLoading;
  errorFromService: Signal<string | null> = this.gameStateService.error;

  // Local UI State Signals (writable)
  selectedCard: WritableSignal<CardDto | null> = signal(null);
  selectedTargetPlayerId: WritableSignal<string | null> = signal(null);
  errorState: WritableSignal<string | null> = signal(null);

  // Computed signal for current player ID
  currentPlayerId: Signal<string | null> = computed(() => this.authService.getCurrentPlayerId());

  targetablePlayers: Signal<{ id: string; name: string; isProtected: boolean }[]> = computed(() => {
    const state = this.spectatorState();
    const myId = this.currentPlayerId();
    if (!state || !myId) return [];
    // Filter out self and eliminated players ONLY
    return state.players
      .filter(p => p.playerId !== myId && p.status === 1) // Check status === 1 (Active)
      .map(p => ({ id: p.playerId, name: p.name, isProtected: p.isProtected }));
  });

  // Computed signal to check if targeting is needed for the selected card
  isTargetingRequired: Signal<boolean> = computed(() => {
    const card = this.selectedCard();
    if (!card) return false;
    const targetTypes = [1, 2, 3, 6]; // Guard, Priest, Baron, King values
    return targetTypes.includes(card.type);
  });

  // Computed signal to check if guessing is needed (Guard)
  isGuessingRequired: Signal<boolean> = computed(() => this.selectedCard()?.type === 1); // Guard value is 1

  // Computed signal for the selected card's name
  selectedCardName: Signal<string> = computed(() => getCardNameFromValue(this.selectedCard()?.type)); // Use helper

  private injector = inject(Injector);
  ngOnInit(): void {
    this.errorState.set(null);
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const id = params.get('id');
        if (id) {
          this.gameStateService.clearState();
          const currentUserId = this.authService.getCurrentPlayerId();
          this.gameStateService.initializeGameConnection(id, currentUserId)
            .catch(err => {
              console.error("GameViewComponent: Error connecting to game:", err);
              this.cdr.markForCheck();
            });
        } else {
          console.error("No game ID found in route.");
          this.errorState.set("No Game ID specified.");
          this.cdr.markForCheck();
        }
      });
    this.subscribeToGameEvents();

    effect(() => {
      const serviceError = this.errorFromService();
      if (serviceError) {
        this.snackBar.open(serviceError, 'Close', { duration: 5000 });
      }
    }, { injector: this.injector });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    const currentId = this.gameId();
    if (currentId) {
      this.gameStateService.disconnectFromGame(currentId);
    }
  }

  private subscribeToGameEvents(): void {
    this.gameStateService.roundWinnerAnnounced$
      .pipe(takeUntil(this.destroy$))
      .subscribe((summaryData: RoundEndSummaryDto) => {
        const winnerName = summaryData.winnerPlayerId ? this.getPlayerName(summaryData.winnerPlayerId) : null;
        this.showSnackBar(`Round Over! ${winnerName ? `${winnerName} wins.` : 'Draw.'} Reason: ${summaryData.reason}`);
        this.openRoundSummaryDialog(summaryData);
      });

    this.gameStateService.gameWinnerAnnounced$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.showSnackBar(`Game Over! Player ${this.getPlayerName(data.winnerId)} wins the game!`, 10000));
  }

  onCardSelected(card: CardDto): void {
    if (!this.isMyTurn() || this.isSpectating()) return;

    if (this.selectedCard()?.appearanceId === card.appearanceId) {
      this.selectedCard.set(null);
      this.selectedTargetPlayerId.set(null);
    } else {
      this.selectedCard.set(card);
      this.selectedTargetPlayerId.set(null);

      if (!this.isTargetingRequired() && !this.isGuessingRequired()) {
        this.confirmAndPlayCard();
      }
    }
  }

  // --- Targeting Interaction ---

  onPlayerSelected(playerId: string): void {
    if (!this.isTargetingRequired() || !this.selectedCard() || this.isSpectating()) return;

    const allOpponents = this.spectatorState()?.players.filter(p => p.playerId !== this.currentPlayerId() && p.status === 1) ?? [];
    const selectedOpponent = allOpponents.find(p => p.playerId === playerId);

    if (!selectedOpponent) {
      console.warn("Attempted to select an invalid or non-existent player.");
      return;
    }

    this.selectedTargetPlayerId.set(playerId);

    if (!this.isGuessingRequired()) {
      this.confirmAndPlayCard();
    } else {
      this.openGuessModal();
    }
  }

  openGuessModal(): void {
    const cardBeingPlayed = this.selectedCard();
    if (!cardBeingPlayed || cardBeingPlayed.type !== 1 || this.isSpectating()) return;

    const dialogData: ActionModalData = {
      actionType: 'guess-card',
      prompt: `Guess Player ${this.getPlayerName(this.selectedTargetPlayerId())}'s card (not Guard):`,
      availableCardTypes: Object.entries(CARD_DETAILS_MAP)
        .map(([valueStr, details]) => ({ value: parseInt(valueStr, 10), name: details.name }))
        .filter(ct => ct.value !== 1), // Exclude Guard (value 1)
      excludeCardTypeValue: 1
    };

    const dialogRef = this.dialog.open<ActionModalComponent, ActionModalData, ActionModalResult>(
      ActionModalComponent, { width: '300px', data: dialogData, disableClose: true }
    );

    dialogRef.afterClosed().subscribe(result => {
      if (result?.selectedCardTypeValue !== undefined) {
        this.confirmAndPlayCard(result.selectedCardTypeValue);
      } else {
        this.selectedCard.set(null);
        this.selectedTargetPlayerId.set(null);
      }
    });
  }

  confirmAndPlayCard(guessedCardValue?: number): void {
    if (this.isSpectating()) return;

    const cardToPlay = this.selectedCard();
    const targetId = this.selectedTargetPlayerId();
    const gameId = this.gameId();
    const myId = this.currentPlayerId();

    if (!cardToPlay || !gameId || !myId) {
      console.error("Cannot play card, missing required state.", { cardToPlay, gameId, myId });
      this.showSnackBar("Error: Cannot determine required game state to play card.", 5000);
      return;
    }

    if (this.isTargetingRequired() && !targetId) {
      this.showSnackBar("Please select a target player.", 3000);
      return;
    }
    if (this.isGuessingRequired() && guessedCardValue === undefined) {
      this.showSnackBar("Please select a card type to guess.", 3000);
      return;
    }

    const payload: PlayCardRequestDto = {
      cardId: cardToPlay.appearanceId,
      targetPlayerId: targetId,
      guessedCardType: guessedCardValue
    };

    this.gameActionService.playCard(gameId, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.selectedCard.set(null);
          this.selectedTargetPlayerId.set(null);
          console.log("PlayCard action sent successfully.");
        },
        error: (err) => {
          this.errorState.set(err.message || 'Failed to play card.');
          this.showSnackBar(`Error: ${this.errorState()}`, 5000);
          this.cdr.markForCheck();
        }
      });
  }

  openRoundSummaryDialog(summaryData: RoundEndSummaryDto): void {
    const dialogRef = this.dialog.open<RoundSummaryDialogComponent, RoundEndSummaryDto, void>(
      RoundSummaryDialogComponent,
      {
        width: '500px', // Adjust as needed
        maxWidth: '90vw',
        data: summaryData,
        disableClose: true
      }
    );

    dialogRef.afterClosed().subscribe(() => {
      console.debug('Round summary dialog closed.');
      this.cdr.markForCheck(); // Ensure UI updates if anything changed indirectly
    });
  }

  onCardInfoClicked(cardRank: number): void {
    console.log('GameView: Card info clicked for rank:', cardRank);
    this.uiInteractionService.requestScrollToCardReference(cardRank);
  }

  // --- Helpers ---
  getPlayerName(playerId: string | null): string {
    if (!playerId) return 'Unknown';
    const state = this.spectatorState();
    const player = state?.players.find(p => p.playerId === playerId);
    return player?.name ?? 'Unknown Player';
  }

  showSnackBar(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Close', { duration });
  }

  // --- TrackBy Functions ---
  trackByIndex(index: number, item: any): number { return index; }
  trackCardById(index: number, item: CardDto): string { return item.appearanceId; }
  trackPlayerById(index: number, item: PlayerHandInfoDto | SpectatorPlayerDto): string { return item.playerId; }
}
