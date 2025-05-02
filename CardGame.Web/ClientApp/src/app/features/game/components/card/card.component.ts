import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import {CardDto} from '../../../../core/models/cardDto'; // Optional: for card styling

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
   * The data for the card to display (ID, Type).
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
   * Emits the CardDto when a face-up card with data is clicked.
   * The parent component determines the action based on game state.
   */
  @Output() cardClicked = new EventEmitter<CardDto>();

  onCardClick(): void {
    // Emit the event if the card is face-up and has data.
    // Let the parent component decide if the click is valid based on isPlayable or game state.
    if (!this.isFaceDown && this.cardData) {
      this.cardClicked.emit(this.cardData);
    }
    // Clicking a face-down card does nothing.
  }

  // Helper to get card type text (could be expanded for icons etc.)
  get cardText(): string {
    return this.cardData?.type ?? '?';
  }

  // Helper to get card rank (assuming type name maps to rank or CardType has rank)
  // This might require more sophisticated mapping based on your CardType setup
  get cardRank(): number | string {
    // Placeholder - map cardData.type to rank
    const typeRankMap: { [key: string]: number } = {
      'Guard': 1, 'Priest': 2, 'Baron': 3, 'Handmaid': 4,
      'Prince': 5, 'King': 6, 'Countess': 7, 'Princess': 8
    };
    return this.cardData?.type ? (typeRankMap[this.cardData.type] ?? '?') : '?';
  }
}
