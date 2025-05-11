import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardComponent} from '../../../card/card.component';
import {CardType} from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

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
  private uiInteractionService = inject(UiInteractionService);

  onCardInfoClicked(cardType: number): void {
    if (cardType) {
      this.uiInteractionService.requestScrollToCardReference(cardType);
    }
  }
}
