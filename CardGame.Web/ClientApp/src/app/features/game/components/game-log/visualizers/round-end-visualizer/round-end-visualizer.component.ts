import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardComponent } from '../../../card/card.component';
import { CardDto } from '../../../../../../core/models/cardDto';

@Component({
  selector: 'app-round-end-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './round-end-visualizer.component.html',
  styleUrls: ['./round-end-visualizer.component.scss']
})
export class RoundEndVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;

  trackCardByAppearanceId(index: number, card: CardDto): string {
    return card.appearanceId;
  }
}
