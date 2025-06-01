import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '@core/models/gameLogEntryDto';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { CardDto } from '@core/models/cardDto';
import { UiInteractionService } from '@core/services/ui-interaction-service.service';

@Component({
  selector: 'app-round-end-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './round-end-visualizer.component.html',
  styleUrls: ['./round-end-visualizer.component.scss']
})
export class RoundEndVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;

  trackCardByAppearanceId(index: number, card: CardDto): string {
    return card.appearanceId;
  }
}
