import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-prince-discard-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './prince-discard-visualizer.component.html',
  styleUrls: ['./prince-discard-visualizer.component.scss']
})
export class PrinceDiscardVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  public CardType = CardType; // Expose CardType to the template

  onPrinceCardInfoClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.Prince);
  }

  onDiscardedCardInfoClicked(): void {
    if (this.logEntry.discardedByPrinceCardValue !== undefined && this.logEntry.discardedByPrinceCardValue !== null) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.discardedByPrinceCardValue);
    }
  }
}
