import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';

@Component({
  selector: 'app-effect-fizzled-visualizer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './effect-fizzled-visualizer.component.html',
  styleUrls: ['./effect-fizzled-visualizer.component.scss']
})
export class EffectFizzledVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
}
