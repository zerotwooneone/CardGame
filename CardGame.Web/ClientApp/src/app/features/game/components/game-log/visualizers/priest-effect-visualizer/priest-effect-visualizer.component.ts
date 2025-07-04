import { Component, Input, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameLogEntryDto } from '@features/game/models/gameLogEntryDto';
import { CardDto } from '@features/game/models/cardDto';
import { UiInteractionService } from '@features/card-reference/services/ui-interaction-service.service';
import { AuthService } from '@features/auth/services/auth.service';
import { CardDisplayComponent } from '@gameComponents/card-display/card-display.component';

@Component({
  selector: 'app-priest-effect-visualizer',
  templateUrl: './priest-effect-visualizer.component.html',
  styleUrls: ['./priest-effect-visualizer.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    CardDisplayComponent
  ]
})
export class PriestEffectVisualizerComponent implements OnInit {
  @Input() logEntry!: GameLogEntryDto;

  private uiInteractionService = inject(UiInteractionService);
  private authService = inject(AuthService);

  currentPlayerIdDisplay: string | null = null;

  ngOnInit(): void {
    this.currentPlayerIdDisplay = this.authService.getCurrentPlayerId();
  }

  get playedPriestCardDisplay(): CardDto | undefined {
    return this.logEntry.playedCard;
  }

  get revealedCardDisplay(): CardDto | undefined {
    return this.logEntry.revealedPlayerCard;
  }

  onPriestCardInfoClicked(): void {
    if (this.playedPriestCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.playedPriestCardDisplay.rank);
    } else {
      // Fallback in case playedCard is unexpectedly undefined for this log type
      this.uiInteractionService.requestScrollToCardReference(2);
    }
  }

  onRevealedCardInfoClicked(): void {
    if (this.revealedCardDisplay?.rank !== undefined) {
      this.uiInteractionService.requestScrollToCardReference(this.revealedCardDisplay.rank);
    }
  }
}
