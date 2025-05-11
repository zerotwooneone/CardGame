import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';

@Component({
  selector: 'app-turn-start-visualizer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './turn-start-visualizer.component.html',
  styleUrls: ['./turn-start-visualizer.component.scss']
})
export class TurnStartVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
}
