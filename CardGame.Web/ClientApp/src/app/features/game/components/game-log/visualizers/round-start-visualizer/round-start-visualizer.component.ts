import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';
import { CardDto } from '../../../../../../core/models/cardDto';

@Component({
  selector: 'app-round-start-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './round-start-visualizer.component.html',
  styleUrls: ['./round-start-visualizer.component.scss']
})
export class RoundStartVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  get initialCardDisplay(): CardDto | undefined {
    // This card could be a burn card, or first player's drawn card, depending on game log specifics
    // return this.logEntry.drawnCard; // ERROR: drawnCard does not exist on GameLogEntryDto for RoundStarted event
    return undefined; // Fix: Return undefined for now. Revisit if initial card data needs to be logged.
  }

  onInitialCardInfoClicked(): void {
    if (this.initialCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.initialCardDisplay.rank);
    }
  }
}
