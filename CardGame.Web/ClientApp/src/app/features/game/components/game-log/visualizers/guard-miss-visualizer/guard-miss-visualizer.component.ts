import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';

@Component({
  selector: 'app-guard-miss-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './guard-miss-visualizer.component.html',
  styleUrls: ['./guard-miss-visualizer.component.scss']
})
export class GuardMissVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  CardType = CardType;
}
