import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { GameLogEntryDto } from '@core/models/gameLogEntryDto';
import { CardType } from '@core/models/cardType';
import { UiInteractionService } from '@core/services/ui-interaction-service.service';
import { CardDto } from '@core/models/cardDto';

@Component({
  selector: 'app-guard-hit-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './guard-hit-visualizer.component.html',
  styleUrls: ['./guard-hit-visualizer.component.scss']
})
export class GuardHitVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  get playedGuardCardDisplay(): CardDto | undefined {
    // This should be the Guard card played by the acting player.
    return this.logEntry.playedCard; 
  }

  // This is the card that was revealed due to the Guard hit.
  get revealedPlayerCardDisplay(): CardDto | undefined {
    return this.logEntry.revealedPlayerCard;
  }

  // Expose the guessed rank for display in the template message
  get guessedRankDisplay(): CardType | undefined {
    return this.logEntry.guessedRank;
  }

  get guessedCardToDisplay(): CardDto | undefined {
    if (this.logEntry.guessedRank !== undefined) {
      // Construct a partial CardDto for display purposes if only rank is known
      return { appearanceId: 'unknown_facedown', rank: this.logEntry.guessedRank };
    }
    return undefined;
  }

  get actualCardToDisplay(): CardDto | undefined {
    return this.revealedPlayerCardDisplay;
  }

  onGuardCardInfoClicked(): void {
    // The playedGuardCardDisplay should have CardType.Guard as its rank if correctly populated
    if (this.playedGuardCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedGuardCardDisplay.rank);
    } else {
      // Fallback if rank somehow isn't on the DTO, though it should be for a Guard card.
      this.uiInteractionService.requestScrollToCardReference(CardType.Guard);
    }
  }

  onRevealedCardInfoClicked(): void {
    if (this.revealedPlayerCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.revealedPlayerCardDisplay.rank);
    }
  }

  onGuessedCardInfoClicked(): void {
    // Use the guessedRank from logEntry for interaction
    if (this.logEntry.guessedRank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.guessedRank);
    }
  }

  onActualCardInfoClicked(): void {
    if (this.revealedPlayerCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.revealedPlayerCardDisplay.rank);
    }
  }
}
