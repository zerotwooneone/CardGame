import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-baron-compare-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './baron-compare-visualizer.component.html',
  styleUrls: ['./baron-compare-visualizer.component.scss']
})
export class BaronCompareVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  onCardInfoClicked(cardType: number): void {
    if (cardType) {
      this.uiInteractionService.requestScrollToCardReference(cardType);
    }
  }
}
