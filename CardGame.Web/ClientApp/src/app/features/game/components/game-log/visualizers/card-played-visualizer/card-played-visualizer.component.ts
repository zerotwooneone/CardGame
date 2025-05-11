import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';

@Component({
  selector: 'app-card-played-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './card-played-visualizer.component.html',
  styleUrls: ['./card-played-visualizer.component.scss']
})
export class CardPlayedVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
}
