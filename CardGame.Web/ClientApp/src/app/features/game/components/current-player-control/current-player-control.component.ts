import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';

import { CardDto } from '../../../../core/models/cardDto';
import { PlayerHandInfoDto } from '../../../../core/models/playerHandInfoDto';
import { SpectatorPlayerDto } from '../../../../core/models/spectatorPlayerDto';
import { CARD_DETAILS_MAP } from '../card-display/CARD_DETAILS_MAP'; // Updated path
import { CardDisplayComponent } from '../card-display/card-display.component';
import { PlayerStatus } from '../player-display/player.status'; // Re-use enum
import { PlayerStatusMap } from '../player-display/player-status.map'; // Re-use map

@Component({
  selector: 'app-current-player-control',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatBadgeModule,
    CardDisplayComponent
  ],
  templateUrl: './current-player-control.component.html',
  styleUrls: ['./current-player-control.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CurrentPlayerControlComponent {
  // --- Host Bindings for dynamic styling ---
  @HostBinding('class.current-turn') get hostIsCurrentTurn(): boolean {
    return this.isMyTurn;
  }

  @HostBinding('class.eliminated') get hostIsEliminated(): boolean {
    return this.playerData?.status === PlayerStatus.Eliminated;
  }

  // --- Inputs ---
  @Input() playerData: PlayerHandInfoDto | SpectatorPlayerDto | null | undefined;
  @Input() handCards: CardDto[] | null | undefined;
  @Input() isMyTurn: boolean = false;
  @Input() isActivePlayer: boolean = false; // True if player status allows actions
  @Input() selectedCard: CardDto | null | undefined;
  @Input() canPlaySelectedCard: boolean = false;
  @Input() cardDetailsMap: typeof CARD_DETAILS_MAP | null = CARD_DETAILS_MAP; // Default to imported map
  @Input() targetPlayerId: string | null = null;
  @Input() guessedCardRank: number | null = null;

  // --- Outputs ---
  @Output() handCardClicked = new EventEmitter<CardDto>();
  @Output() playCardClicked = new EventEmitter<void>();
  @Output() targetPlayerSelected = new EventEmitter<string>();
  @Output() cardGuessConfirmed = new EventEmitter<number>();

  // Expose PlayerStatus enum to the template
  public PlayerStatus = PlayerStatus;

  // --- Helpers ---
  get discardPileCards(): CardDto[] {
    if (this.playerData && 'playedCards' in this.playerData) {
      return this.playerData.playedCards ?? [];
    }
    return [];
  }

  get playerStatusName(): string {
    const statusValue = this.playerData?.status as PlayerStatus | undefined;
    if (statusValue === undefined || !PlayerStatusMap[statusValue]) {
      return PlayerStatusMap[PlayerStatus.Unknown];
    }
    return PlayerStatusMap[statusValue];
  }

  get selectedCardName(): string | null {
    if (this.selectedCard && this.cardDetailsMap && this.cardDetailsMap[this.selectedCard.rank]) {
      return this.cardDetailsMap[this.selectedCard.rank].name;
    }
    return null;
  }

  // --- Event Handlers to emit outputs ---
  onHandCardClicked(card: CardDto): void {
    this.handCardClicked.emit(card);
  }

  onPlayCardClicked(): void {
    this.playCardClicked.emit();
  }

  // --- TrackBy Functions ---
  trackByIndex(index: number, item: any): number {
    return index;
  }

  trackCardById(index: number, item: CardDto): string {
    // Use appearanceId for unique tracking. Fallback to rank and index if needed.
    return item.appearanceId ?? `${item.rank}-${index}`;
  }
}
