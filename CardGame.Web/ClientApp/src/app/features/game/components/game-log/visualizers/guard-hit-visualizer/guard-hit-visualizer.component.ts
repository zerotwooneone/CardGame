import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';

@Component({
  selector: 'app-guard-hit-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './guard-hit-visualizer.component.html',
  styleUrls: ['./guard-hit-visualizer.component.scss']
})
export class GuardHitVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  CardType = CardType;
}
