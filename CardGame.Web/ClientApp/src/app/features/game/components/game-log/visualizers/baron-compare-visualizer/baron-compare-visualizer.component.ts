import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-baron-compare-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './baron-compare-visualizer.component.html',
  styleUrls: ['./baron-compare-visualizer.component.scss']
})
export class BaronCompareVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  onPlayer1ComparedCardInfoClicked() {
    if (this.logEntry.player1ComparedCardValue !== undefined && this.logEntry.player1ComparedCardValue !== null) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.player1ComparedCardValue);
    }
  }

  onPlayer2ComparedCardInfoClicked() {
    if (this.logEntry.player2ComparedCardValue !== undefined && this.logEntry.player2ComparedCardValue !== null) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.player2ComparedCardValue);
    }
  }

  protected readonly CardType = CardType;
}
