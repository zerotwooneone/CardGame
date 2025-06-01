import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';

@Component({
  selector: 'app-token-awarded-visualizer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './token-awarded-visualizer.component.html',
  styleUrls: ['./token-awarded-visualizer.component.scss']
})
export class TokenAwardedVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
}
