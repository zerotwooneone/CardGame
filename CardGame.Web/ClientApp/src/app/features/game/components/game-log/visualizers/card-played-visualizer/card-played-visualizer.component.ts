import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';
import { CardDto } from '../../../../../../core/models/cardDto';

@Component({
  selector: 'app-card-played-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './card-played-visualizer.component.html',
  styleUrls: ['./card-played-visualizer.component.scss']
})
export class CardPlayedVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  get playedCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  onCardInfoClicked(): void { 
    if (this.playedCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedCardDisplay.rank);
    }
  }
}
