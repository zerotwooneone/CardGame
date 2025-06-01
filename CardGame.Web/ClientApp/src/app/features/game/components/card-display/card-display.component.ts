import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, computed, signal, WritableSignal, SimpleChanges, OnChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { CardDto } from '../../models/cardDto';
import { CARD_DETAILS_MAP } from './CARD_DETAILS_MAP';
import { DeckService } from '@gameServices/deck.service';
import { UiInteractionService } from '../../../card-reference/services/ui-interaction-service.service';

@Component({
  selector: 'app-card-display',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatCardModule
  ],
  templateUrl: './card-display.component.html',
  styleUrls: ['./card-display.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardDisplayComponent implements OnChanges {
  @Input() card: CardDto | undefined;
  @Input() isFaceDown: boolean = false;
  @Input() isPlayable: boolean = false;
  @Input() isSelected: boolean = false;
  @Input() isHovered: boolean = false;

  @Output() cardClicked = new EventEmitter<CardDto>();
  @Output() mouseEnter = new EventEmitter<CardDto>();
  @Output() mouseLeave = new EventEmitter<CardDto>();

  public imageError: WritableSignal<boolean> = signal(false);
  public CARD_DETAILS_MAP = CARD_DETAILS_MAP; // Expose to template

  private deckService = inject(DeckService);
  private uiInteractionService = inject(UiInteractionService);
  public cardBackImageSignal = this.deckService.backAppearanceId;


  handleImageError(): void {
    this.imageError.set(true);
  }

  onCardClick(): void {
    if (this.isPlayable && !this.isFaceDown && this.card) {
      this.cardClicked.emit(this.card);
    }
  }

  onInfoClick(event: MouseEvent): void {
    event.stopPropagation();
    if (this.card && typeof this.card.rank !== 'undefined') {
      this.uiInteractionService.requestScrollToCardReference(this.card.rank);
    }
  }

  onMouseEnter(): void {
    if (this.card) {
      this.mouseEnter.emit(this.card);
    }
  }

  onMouseLeave(): void {
    if (this.card) {
      this.mouseLeave.emit(this.card);
    }
  }

  readonly altText = computed(() => {
    if (this.isFaceDown) return 'Card face down';
    if (!this.card || typeof this.card.rank === 'undefined') {
      return 'Playing card';
    }
    const details = CARD_DETAILS_MAP[this.card.rank];
    return details
      ? `Card: ${this.card.rank} - ${details.name}`
      : `Card: ${this.card.rank} - Unknown`;
  });

  readonly cardFrontBackgroundImage = computed(() => {
    if (this.isFaceDown || this.imageError() || !this.card?.appearanceId) {
      return 'none';
    }
    return `url('${this.card.appearanceId}')`;
  });

  readonly cardFrontFallbackText = computed(() => {
    if (!this.card || typeof this.card.rank === 'undefined') {
      return '?';
    }
    const details = CARD_DETAILS_MAP[this.card.rank];
    return details ? `${details.name}\n(Rank: ${this.card.rank})` : `Rank: ${this.card.rank}`;
  });

  readonly fallbackDisplayText = computed(() => {
    if (!this.card || typeof this.card.rank === 'undefined') {
      return 'Unknown Card';
    }
    const details = CARD_DETAILS_MAP[this.card.rank];
    return details
      ? `${details.name} (Rank: ${this.card.rank})`
      : `Rank: ${this.card.rank}`;
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['card'] && !changes['card'].firstChange) {
      this.imageError.set(false);
    }
  }
}
