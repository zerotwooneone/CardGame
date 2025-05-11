import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';

@Component({
  selector: 'app-handmaid-protection-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './handmaid-protection-visualizer.component.html',
  styleUrls: ['./handmaid-protection-visualizer.component.scss']
})
export class HandmaidProtectionVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  protected readonly CardType = CardType;
}
