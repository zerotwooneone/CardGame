import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-prince-discard-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './prince-discard-visualizer.component.html',
  styleUrls: ['./prince-discard-visualizer.component.scss']
})
export class PrinceDiscardVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  public CardType = CardType;
  private uiInteractionService = inject(UiInteractionService);

  onPrinceCardInfoClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.Prince);
  }

  onDiscardedCardInfoClicked(): void {
    if (this.logEntry.discardedByPrinceCardValue) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.discardedByPrinceCardValue);
    }
  }
}
