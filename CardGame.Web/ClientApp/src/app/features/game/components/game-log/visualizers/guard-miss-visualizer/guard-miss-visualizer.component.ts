import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';
import { UiInteractionService } from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-guard-miss-visualizer',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './guard-miss-visualizer.component.html',
  styleUrls: ['./guard-miss-visualizer.component.scss']
})
export class GuardMissVisualizerComponent {
  @Input() logEntry!: GameLogEntryDto;
  private uiInteractionService = inject(UiInteractionService);
  public CardType = CardType;

  onGuardCardInfoClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.Guard);
  }

  onGuessedCardInfoClicked(): void {
    if (this.logEntry.guessedCardValue) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.guessedCardValue);
    }
  }
}
