import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { CardRank } from '@features/game/models/cardRank';
import { CardDto } from '@features/game/models/cardDto';

@Component({
  selector: 'app-handmaid-protection-visualizer',
  standalone: true,
  imports: [CommonModule, CardDisplayComponent],
  templateUrl: './handmaid-protection-visualizer.component.html',
  styleUrls: ['./handmaid-protection-visualizer.component.scss']
})
export class HandmaidProtectionVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);

  get playedHandmaidCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  onHandmaidCardDisplayClicked(): void {
    if (this.playedHandmaidCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedHandmaidCardDisplay.rank);
    } else {
      this.uiInteractionService.requestScrollToCardReference(CardRank.Handmaid);
    }
  }
}
