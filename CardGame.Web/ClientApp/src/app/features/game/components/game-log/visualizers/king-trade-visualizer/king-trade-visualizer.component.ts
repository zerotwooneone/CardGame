import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';
import { CardRank } from '@features/game/models/cardRank';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { CardDto } from '@features/game/models/cardDto';

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

  get playedKingCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  get revealedTradedCardDisplay(): CardDto | undefined {
    return this.logEntry.revealedTradedCard;
  }

  onKingCardInfoClicked(): void {
    if (this.playedKingCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedKingCardDisplay.rank);
    } else {
      // Fallback in case playedCard is unexpectedly undefined for this log type
      this.uiInteractionService.requestScrollToCardReference(CardRank.King);
    }
  }

  onTradedCardInfoClicked(): void {
    if (this.revealedTradedCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.revealedTradedCardDisplay.rank);
    }
  }
}
