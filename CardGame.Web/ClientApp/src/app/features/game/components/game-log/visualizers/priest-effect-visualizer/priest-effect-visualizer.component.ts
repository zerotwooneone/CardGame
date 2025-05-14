import {Component, Input, inject} from '@angular/core';
import {CommonModule} from '@angular/common';
import {CardDisplayComponent} from '../../../../../../shared/components/card-display.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';
import {UiInteractionService} from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-priest-effect-visualizer',
  templateUrl: './priest-effect-visualizer.component.html',
  styleUrls: ['./priest-effect-visualizer.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    CardDisplayComponent
  ]
})
export class PriestEffectVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;

  public CardType = CardType;
  private uiInteractionService = inject(UiInteractionService);

  onPriestCardInfoClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.Priest);
  }

  onRevealedCardInfoClicked(): void {
    if (this.logEntry.revealedCardValue !== undefined && this.logEntry.revealedCardValue !== null) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.revealedCardValue);
    }
  }
}
