import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { GameLogEntryDto } from '@core/models/gameLogEntryDto';
import { CardType } from '@core/models/cardType';
import { UiInteractionService } from '@core/services/ui-interaction-service.service';
import { CardDto } from '@core/models/cardDto';

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

  get player1CardDisplay(): CardDto | undefined { // Acting player
    return this.logEntry.actingPlayerBaronCard;
  }

  get player2CardDisplay(): CardDto | undefined { // Target player
    return this.logEntry.targetPlayerBaronCard;
  }

  onPlayer1ComparedCardInfoClicked(): void {
    if (this.player1CardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.player1CardDisplay.rank);
    }
  }

  onPlayer2ComparedCardInfoClicked(): void {
    if (this.player2CardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.player2CardDisplay.rank);
    }
  }

  protected readonly CardType = CardType;
}
