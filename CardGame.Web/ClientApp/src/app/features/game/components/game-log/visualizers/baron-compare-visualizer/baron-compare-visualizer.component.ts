import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';

@Component({
  selector: 'app-baron-compare-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './baron-compare-visualizer.component.html',
  styleUrls: ['./baron-compare-visualizer.component.scss']
})
export class BaronCompareVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
}
