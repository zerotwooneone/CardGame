import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';

@Component({
  selector: 'app-prince-discard-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './prince-discard-visualizer.component.html',
  styleUrls: ['./prince-discard-visualizer.component.scss']
})
export class PrinceDiscardVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  protected readonly CardType = CardType;
}
