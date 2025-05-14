import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDto } from '../../core/models/cardDto'; // Adjusted path assuming it's now in 'components'

@Component({
  selector: 'app-card-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card-display.component.html',
  styleUrls: ['./card-display.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardDisplayComponent {
  @Input() card: CardDto | undefined;

  constructor() { }

  getCardImageUrl(): string {
    if (!this.card || !this.card.appearanceId) {
      // Return a path to a default 'card back' image or an empty string
      return 'assets/images/cards/card_back.png';
    }

    return this.card.appearanceId;
  }
}
