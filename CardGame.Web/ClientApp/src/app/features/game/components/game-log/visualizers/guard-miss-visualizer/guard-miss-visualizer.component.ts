import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';
import { CardDto } from '../../../../../../core/models/cardDto';

@Component({
  selector: 'app-guard-miss-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './guard-miss-visualizer.component.html',
  styleUrls: ['./guard-miss-visualizer.component.scss']
})
export class GuardMissVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  get playedGuardCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  get guessedCardToDisplay(): CardDto | undefined {
    if (this.logEntry.guessedRank !== undefined) {
      // Construct a partial CardDto for display purposes if only rank is known
      return { appearanceId: 'unknown_facedown', rank: this.logEntry.guessedRank };
    }
    return undefined;
  }

  onGuardCardInfoClicked(): void {
    if (this.playedGuardCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedGuardCardDisplay.rank);
    } else {
      this.uiInteractionService.requestScrollToCardReference(CardType.Guard); 
    }
  }

  onGuessedCardInfoClicked(): void {
    // Use the guessedRank from logEntry for interaction
    if (this.logEntry.guessedRank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.guessedRank);
    }
  }
}
