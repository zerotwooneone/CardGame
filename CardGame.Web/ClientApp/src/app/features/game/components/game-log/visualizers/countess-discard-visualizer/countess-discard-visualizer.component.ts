import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';

@Component({
  selector: 'app-countess-discard-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './countess-discard-visualizer.component.html',
  styleUrls: ['./countess-discard-visualizer.component.scss']
})
export class CountessDiscardVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  protected readonly CardType = CardType;
}
