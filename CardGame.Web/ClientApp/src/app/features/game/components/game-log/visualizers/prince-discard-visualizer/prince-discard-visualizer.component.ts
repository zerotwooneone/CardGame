import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';
import { CardType } from '@features/game/models/cardType';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { CardDto } from '@features/game/models/cardDto';

@Component({
  selector: 'app-prince-discard-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './prince-discard-visualizer.component.html',
  styleUrls: ['./prince-discard-visualizer.component.scss']
})
export class PrinceDiscardVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  public CardType = CardType; // Expose CardType to the template

  get playedPrinceCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  get targetDiscardedCardDisplay(): CardDto | undefined {
    return this.logEntry.targetDiscardedCard;
  }

  get targetNewCardDisplay(): CardDto | undefined {
    return this.logEntry.targetNewCardAfterPrince;
  }

  onPrinceCardInfoClicked(): void {
    if (this.playedPrinceCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedPrinceCardDisplay.rank);
    } else {
      this.uiInteractionService.requestScrollToCardReference(CardType.Prince); // Fallback
    }
  }

  onTargetDiscardedCardInfoClicked(): void {
    if (this.targetDiscardedCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.targetDiscardedCardDisplay.rank);
    }
  }

  onTargetNewCardInfoClicked(): void {
    if (this.targetNewCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.targetNewCardDisplay.rank);
    }
  }
}
