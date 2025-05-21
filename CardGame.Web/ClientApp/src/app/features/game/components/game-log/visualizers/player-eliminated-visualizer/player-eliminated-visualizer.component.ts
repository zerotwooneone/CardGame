import { Component, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';
import { CardDto } from '../../../../../../core/models/cardDto';

@Component({
  selector: 'app-player-eliminated-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './player-eliminated-visualizer.component.html',
  styleUrls: ['./player-eliminated-visualizer.component.scss']
})
export class PlayerEliminatedVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  // Card played by actingPlayer that led to elimination (e.g., Baron, Guard)
  get eliminatingCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  // Card revealed by targetPlayer that confirmed elimination (e.g., lower Baron card, correct Guard guess)
  get revealedPlayerCardDisplay(): CardDto | undefined {
    return this.logEntry.revealedCardOnElimination;
  }

  onEliminatingCardInfoClicked(): void {
    if (this.eliminatingCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.eliminatingCardDisplay.rank);
    }
  }

  onRevealedPlayerCardInfoClicked(): void {
    if (this.revealedPlayerCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.revealedPlayerCardDisplay.rank);
    }
  }
}
