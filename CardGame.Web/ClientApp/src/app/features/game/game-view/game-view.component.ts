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
import {SpectatorGameStateDto} from '../../../core/models/spectatorGameStateDto'; // Adjust pathm/class representation if needed for mapping
import { CardDto } from '../../../core/models/cardDto';
import {ActionModalData} from '../actionModalData';
import {ActionModalResult} from '../actionModalResult';
import {PlayCardRequestDto} from '../../../core/models/playCardRequestDto';
import {SpectatorPlayerDto} from '../../../core/models/spectatorPlayerDto';
import {CARD_DETAILS_MAP} from '../components/card/CARD_DETAILS_MAP';
import {UiInteractionService} from '../../../core/services/ui-interaction-service.service';

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
  spectatorState: Signal<SpectatorGameStateDto | null> = this.gameStateService.spectatorState;
  playerHand: Signal<CardDto[]> = this.gameStateService.playerHand;
  isMyTurn: Signal<boolean> = this.gameStateService.isMyTurn;
  gamePhase: Signal<string | null> = this.gameStateService.gamePhase;
  gameId: Signal<string | null> = this.gameStateService.gameId;

  // Local UI State Signals (writable)
  selectedCard: WritableSignal<CardDto | null> = signal(null);
  selectedTargetPlayerId: WritableSignal<string | null> = signal(null);
  isLoadingAction: WritableSignal<boolean> = signal(false);
  errorState: WritableSignal<string | null> = signal(null); // Use this signal for errors

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

  ngOnInit(): void {
    this.errorState.set(null); // Clear error on init

    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const id = params.get('id');
        if (id) {
          this.gameStateService.clearState(); // Clear previous state
          this.gameStateService.connectToGame(id)
            .catch(err => {
              console.error("Error connecting to game:", err);
              this.errorState.set("Could not connect to the game.");
              this.snackBar.open('Error connecting to game.', 'Close', { duration: 3000 });
              this.cdr.markForCheck(); // Trigger change detection for error message
            });
        } else {
          console.error("No game ID found in route.");
          this.errorState.set("No Game ID specified.");
        }
      });

    this.subscribeToGameEvents();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    const currentId = this.gameId(); // Read signal value
    if (currentId) {
      this.gameStateService.disconnectFromGame(currentId);
    }
  }

  private subscribeToGameEvents(): void {
    // Use helper function for card names in snackbar messages
    this.gameStateService.playerGuessed$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.showSnackBar(`Guess (${getCardNameFromValue(data.guessedCardTypeValue)}) result: ${data.wasCorrect ? 'Correct!' : 'Incorrect.'}`));

    this.gameStateService.playersComparedHands$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.showSnackBar(`Baron: ${data.loserId ? `Player ${this.getPlayerName(data.loserId)} is out!` : 'Tie!'}`));

    this.gameStateService.playerDiscarded$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.showSnackBar(`Player ${this.getPlayerName(data.targetPlayerId)} discarded ${getCardNameFromValue(data.discardedCard.type)}`)); // Use helper

    this.gameStateService.cardsSwapped$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.showSnackBar(`Player ${this.getPlayerName(data.player1Id)} swapped hands with Player ${this.getPlayerName(data.player2Id)}`));

    this.gameStateService.roundWinnerAnnounced$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.showSnackBar(`Round Over! ${data.winnerId ? `Player ${this.getPlayerName(data.winnerId)} wins.` : 'Draw.'} Reason: ${data.reason}`));

    this.gameStateService.gameWinnerAnnounced$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.showSnackBar(`Game Over! Player ${this.getPlayerName(data.winnerId)} wins the game!`, 10000));

    this.gameStateService.cardEffectFizzled$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        const actorName = this.getPlayerName(data.actorId);
        const targetName = this.getPlayerName(data.targetId);
        const cardName = getCardNameFromValue(data.cardTypeValue);
        this.showSnackBar(`${actorName}'s ${cardName} had no effect on ${targetName} (${data.reason}).`);
      });
  }

  // --- Card Interaction ---

  onCardSelected(card: CardDto): void {
    if (!this.isMyTurn()) return;

    if (this.selectedCard()?.id === card.id) {
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
    if (!this.isTargetingRequired() || !this.selectedCard()) return;

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

  // --- Action Confirmation / Modal ---

  openGuessModal(): void {
    const cardBeingPlayed = this.selectedCard();
    if (!cardBeingPlayed || cardBeingPlayed.type !== 1) return;

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

  // --- Play Action ---

  confirmAndPlayCard(guessedValue?: number): void {
    const card = this.selectedCard();
    const targetId = this.selectedTargetPlayerId();
    const gameId = this.gameId();
    const myId = this.currentPlayerId();

    if (!card || !gameId || !myId) {
      console.error("Cannot play card, missing required state.", { card, gameId, myId });
      this.showSnackBar("Error: Cannot determine required game state to play card.", 5000);
      return;
    }

    if (this.isTargetingRequired() && !targetId) {
      this.showSnackBar("Please select a target player.", 3000);
      return;
    }
    if (this.isGuessingRequired() && guessedValue === undefined) {
      this.showSnackBar("Please select a card type to guess.", 3000);
      return;
    }

    this.isLoadingAction.set(true);
    this.errorState.set(null);

    const payload: PlayCardRequestDto = {
      cardId: card.id,
      targetPlayerId: targetId,
      guessedCardType: guessedValue
    };

    this.gameActionService.playCard(gameId, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isLoadingAction.set(false);
          this.selectedCard.set(null);
          this.selectedTargetPlayerId.set(null);
          console.log("PlayCard action sent successfully.");
        },
        error: (err) => {
          this.isLoadingAction.set(false);
          this.errorState.set(err.message || 'Failed to play card.');
          this.showSnackBar(`Error: ${this.errorState()}`, 5000);
          this.cdr.markForCheck();
        }
      });
  }

  onCardInfoClicked(cardRank: number): void {
    console.log('GameView: Card info clicked for rank:', cardRank);
    this.uiInteractionService.requestScrollToCardReference(cardRank);
  }

  // --- Helpers ---
  getPlayerName(playerId: string | null | undefined): string {
    if (!playerId) return 'Unknown';
    return this.spectatorState()?.players.find(p => p.playerId === playerId)?.name ?? 'Unknown';
  }

  showSnackBar(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Close', { duration });
  }

  // --- TrackBy Functions ---
  trackByIndex(index: number, item: any): number { return index; }
  trackCardById(index: number, item: CardDto): string { return item.id; }
  trackPlayerById(index: number, item: SpectatorPlayerDto): string {
    return item.playerId;
  }

}
