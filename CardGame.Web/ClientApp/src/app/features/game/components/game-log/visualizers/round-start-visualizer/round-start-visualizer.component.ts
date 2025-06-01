import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { CardDto } from '@features/game/models/cardDto';

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
