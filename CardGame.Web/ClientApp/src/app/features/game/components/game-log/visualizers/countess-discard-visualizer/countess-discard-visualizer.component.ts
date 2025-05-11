import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-countess-discard-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './countess-discard-visualizer.component.html',
  styleUrls: ['./countess-discard-visualizer.component.scss']
})
export class CountessDiscardVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  protected readonly CardType = CardType;
  private uiInteractionService = inject(UiInteractionService);

  onCardInfoClicked(cardType: number): void {
    if (cardType) {
      this.uiInteractionService.requestScrollToCardReference(cardType);
    }
  }
}
