import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-card-played-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './card-played-visualizer.component.html',
  styleUrls: ['./card-played-visualizer.component.scss']
})
export class CardPlayedVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  onCardInfoClicked(cardType: number): void {
    if (cardType) { 
      this.uiInteractionService.requestScrollToCardReference(cardType);
    }
  }
}
