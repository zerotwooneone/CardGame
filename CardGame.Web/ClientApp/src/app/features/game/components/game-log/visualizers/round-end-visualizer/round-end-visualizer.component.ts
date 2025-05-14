import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { CardDto } from '../../../../../../core/models/cardDto';

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
