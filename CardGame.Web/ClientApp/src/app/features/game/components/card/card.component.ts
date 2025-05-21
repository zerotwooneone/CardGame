import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MatCardModule} from '@angular/material/card';
import {CardDto} from '../../../../core/models/cardDto';
import {CARD_DETAILS_MAP} from './CARD_DETAILS_MAP';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip'; // Optional: for card styling

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ],
  templateUrl: './card.component.html',
  styleUrls: ['./card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardComponent {
  /**
   * The data for the card to display (ID, numeric Type/Rank).
   * Undefined if the card is face down or represents an unknown card.
   */
  @Input() cardData?: CardDto;

  /**
   * Whether the card should be displayed face down.
   */
  @Input() isFaceDown: boolean = true;

  /**
   * Whether the card is currently selected by the player.
   */
  @Input() isSelected: boolean = false;

  /**
   * Whether the card is currently playable (e.g., in the current player's hand during their turn).
   */
  @Input() isPlayable: boolean = false;

  /**
   * Emits the CardDto when a playable card is clicked.
   */
  @Output() cardClicked = new EventEmitter<CardDto>();

  /**
   * Emits the card's rank (type value) when the info icon is clicked.
   */
  @Output() infoClicked = new EventEmitter<number>(); // New output

  onCardClick(): void {
    // Emit the event if the card is playable, face-up, and has data.
    if (this.isPlayable && !this.isFaceDown && this.cardData) {
      this.cardClicked.emit(this.cardData);
    }
  }

  /**
   * Handles the click on the info icon.
   * Emits the card's type (rank) if the card data is available.
   * Stops event propagation to prevent the cardClick from also firing.
   * @param event The mouse event.
   */
  onInfoClick(event: MouseEvent): void {
    event.stopPropagation(); // Prevent cardClick from also firing
    if (this.cardData?.rank) {
      this.infoClicked.emit(this.cardData.rank);
    }
  }

  /**
   * Gets the display name of the card based on its numeric type value.
   */
  get cardText(): string {
    return this.cardData?.rank ? (CARD_DETAILS_MAP[this.cardData.rank]?.name ?? '?') : '?';
  }

  /**
   * Gets the rank of the card, which is now directly the numeric type value.
   */
  get cardRank(): number | string {
    return this.cardData?.rank ?? '?';
  }
}
