import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MatCardModule} from '@angular/material/card';
import {CardDto} from '../../../../core/models/cardDto';
import {CARD_DETAILS_MAP} from './CARD_DETAILS_MAP'; // Optional: for card styling


@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule, MatCardModule], // Import MatCardModule if using mat-card
  templateUrl: './card.component.html',
  styleUrls: ['./card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush // Good for presentational components
})
export class CardComponent {
  /**
   * The data for the card to display (ID, numeric Type/Rank).
   * Undefined if the card is face down or represents an unknown card.
   */
  @Input() cardData?: CardDto; // CardDto.type is now number

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
   * Emits the CardDto when a face-up card with data is clicked.
   */
  @Output() cardClicked = new EventEmitter<CardDto>();

  onCardClick(): void {
    // Emit the event if the card is face-up and has data.
    if (!this.isFaceDown && this.cardData) {
      this.cardClicked.emit(this.cardData);
    }
  }

  /**
   * Gets the display name of the card based on its numeric type value.
   */
  get cardText(): string {
    // Use the map to find the name based on the numeric type
    return this.cardData?.type ? (CARD_DETAILS_MAP[this.cardData.type]?.name ?? '?') : '?';
  }

  /**
   * Gets the rank of the card, which is now directly the numeric type value.
   */
  get cardRank(): number | string {
    // The type property itself is the rank/value
    return this.cardData?.type ?? '?';
  }
}
