import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';

@Component({
  selector: 'app-round-end-visualizer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './round-end-visualizer.component.html',
  styleUrls: ['./round-end-visualizer.component.scss']
})
export class RoundEndVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
}
