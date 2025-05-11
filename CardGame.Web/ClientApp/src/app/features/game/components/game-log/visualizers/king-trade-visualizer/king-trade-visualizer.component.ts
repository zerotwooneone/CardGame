import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-king-trade-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './king-trade-visualizer.component.html',
  styleUrls: ['./king-trade-visualizer.component.scss']
})
export class KingTradeVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  CardType = CardType;

  onCardInfoClicked(cardType: number): void {
    if (cardType) {
      this.uiInteractionService.requestScrollToCardReference(cardType);
    }
  }
}
