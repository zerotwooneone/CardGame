import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '../../../../../../shared/components/card-display.component';
import { GameLogEntryDto } from '../../../../../../core/models/gameLogEntryDto';
import { CardType } from '../../../../../../core/models/cardType';
import { CardDto } from '../../../../../../core/models/cardDto';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-countess-discard-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './countess-discard-visualizer.component.html',
  styleUrls: ['./countess-discard-visualizer.component.scss']
})
export class CountessDiscardVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  // This is the Prince or King card that was played by the player
  get playedPrinceOrKingCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  // This is the Countess card that was forcibly discarded
  get discardedCountessCardDisplay(): CardDto | undefined {
    return this.logEntry.discardedCard;
  }

  onPlayedPrinceOrKingCardInfoClicked(): void {
    // playedCard could be Prince or King
    if (this.playedPrinceOrKingCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedPrinceOrKingCardDisplay.rank);
    } 
  }

  onDiscardedCountessCardInfoClicked(): void {
    // discardedCard should be Countess in this scenario
    if (this.discardedCountessCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.discardedCountessCardDisplay.rank);
    } else {
      // Fallback just in case, though it should be Countess
      this.uiInteractionService.requestScrollToCardReference(CardType.Countess);
    }
  }
}
