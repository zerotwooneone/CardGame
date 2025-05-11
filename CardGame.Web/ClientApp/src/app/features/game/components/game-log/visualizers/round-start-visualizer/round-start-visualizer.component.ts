import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';

@Component({
  selector: 'app-round-start-visualizer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './round-start-visualizer.component.html',
  styleUrls: ['./round-start-visualizer.component.scss']
})
export class RoundStartVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
}
