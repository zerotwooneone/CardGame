import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';
import { CardDto } from '../../../../../../core/models/cardDto';

@Component({
  selector: 'app-turn-start-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './turn-start-visualizer.component.html',
  styleUrls: ['./turn-start-visualizer.component.scss']
})
export class TurnStartVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  get drawnCardDisplay(): CardDto | undefined {
    // return this.logEntry.drawnCard; // ERROR: drawnCard does not exist on GameLogEntryDto for TurnStarted event.
                                     // The actual card draw is a separate log entry.
    return undefined; // Fix: Return undefined for now.
  }

  onDrawnCardInfoClicked(): void {
    if (this.drawnCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.drawnCardDisplay.rank);
    }
  }
}
