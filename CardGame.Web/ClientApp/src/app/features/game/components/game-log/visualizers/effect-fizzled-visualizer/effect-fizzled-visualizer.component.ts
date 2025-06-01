import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { CardDto } from '@features/game/models/cardDto';

@Component({
  selector: 'app-effect-fizzled-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './effect-fizzled-visualizer.component.html',
  styleUrls: ['./effect-fizzled-visualizer.component.scss']
})
export class EffectFizzledVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  get fizzledCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  onFizzledCardInfoClicked(): void {
    if (this.fizzledCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.fizzledCardDisplay.rank);
    }
  }
}
