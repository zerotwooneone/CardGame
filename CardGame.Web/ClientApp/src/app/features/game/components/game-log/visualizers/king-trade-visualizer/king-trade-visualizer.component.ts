import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-king-trade-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './king-trade-visualizer.component.html',
  styleUrls: ['./king-trade-visualizer.component.scss']
})
export class KingTradeVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  public CardType = CardType;

  onKingCardInfoClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.King);
  }

  // This method is called by (infoClicked) from app-card-display for the traded card
  onTradedCardInfoClicked(cardValue: CardType | undefined): void {
    if (cardValue !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(cardValue);
    }
  }
}
