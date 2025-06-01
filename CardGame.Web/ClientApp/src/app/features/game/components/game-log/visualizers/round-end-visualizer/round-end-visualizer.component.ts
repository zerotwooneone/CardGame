import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';
import { CardDto } from '@features/game/models/cardDto';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';

@Component({
  selector: 'app-round-end-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './round-end-visualizer.component.html',
  styleUrls: ['./round-end-visualizer.component.scss']
})
export class RoundEndVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  trackCardByAppearanceId(index: number, card: CardDto): string {
    return card.appearanceId;
  }
}
