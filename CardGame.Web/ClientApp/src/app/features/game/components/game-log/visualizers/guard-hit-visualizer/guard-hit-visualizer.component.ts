import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-guard-hit-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './guard-hit-visualizer.component.html',
  styleUrls: ['./guard-hit-visualizer.component.scss']
})
export class GuardHitVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);
  public CardType = CardType;

  onGuardCardInfoClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.Guard);
  }

  onGuessedCardInfoClicked(): void {
    if (this.logEntry.guessedCardValue) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.guessedCardValue);
    }
  }
}
