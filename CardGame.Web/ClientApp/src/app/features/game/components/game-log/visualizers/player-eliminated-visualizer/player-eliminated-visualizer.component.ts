import { Component, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';
import { CardType } from '../../../../../../core/models/cardType';

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

  public CardType = CardType; // Expose CardType to the template

  onCardInfoClicked(cardValue: CardType | undefined): void {
    if (cardValue !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(cardValue);
    }
  }

  onSourceCardInfoClicked(): void {
    if (this.logEntry.cardResponsibleForEliminationValue !== undefined && this.logEntry.cardResponsibleForEliminationValue !== null) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.cardResponsibleForEliminationValue);
    }
  }
}
