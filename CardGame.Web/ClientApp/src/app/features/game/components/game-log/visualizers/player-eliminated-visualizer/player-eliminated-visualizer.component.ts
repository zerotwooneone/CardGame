import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardComponent} from '../../../card/card.component';
import {CardType} from '../../../../../../core/models/cardType';

@Component({
  selector: 'app-player-eliminated-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './player-eliminated-visualizer.component.html',
  styleUrls: ['./player-eliminated-visualizer.component.scss']
})
export class PlayerEliminatedVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  protected readonly CardType = CardType;
}
