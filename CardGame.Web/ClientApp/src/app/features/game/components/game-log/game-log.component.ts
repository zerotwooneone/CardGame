import {
  Component,
  ChangeDetectionStrategy,
  inject,
  Signal,
  ElementRef,
  ViewChild,
  AfterViewChecked,
  effect
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { GameStateService } from '../../services/game-state.service';
import { GameLogEntryDto } from '../../../../core/models/gameLogEntryDto';
import { CARD_DETAILS_MAP } from '../card/CARD_DETAILS_MAP'; // For card names
import { PriestEffectVisualizerComponent } from './visualizers/priest-effect-visualizer/priest-effect-visualizer.component';
import { HandmaidProtectionVisualizerComponent } from './visualizers/handmaid-protection-visualizer/handmaid-protection-visualizer.component';
import { EffectFizzledVisualizerComponent } from './visualizers/effect-fizzled-visualizer/effect-fizzled-visualizer.component';
import { PlayerEliminatedVisualizerComponent } from './visualizers/player-eliminated-visualizer/player-eliminated-visualizer.component';
import { BaronCompareVisualizerComponent } from './visualizers/baron-compare-visualizer/baron-compare-visualizer.component';
import { PrinceDiscardVisualizerComponent } from './visualizers/prince-discard-visualizer/prince-discard-visualizer.component';
import { KingTradeVisualizerComponent } from './visualizers/king-trade-visualizer/king-trade-visualizer.component';
import { CountessDiscardVisualizerComponent } from './visualizers/countess-discard-visualizer/countess-discard-visualizer.component';
import { RoundEndVisualizerComponent } from './visualizers/round-end-visualizer/round-end-visualizer.component';
import { TokenAwardedVisualizerComponent } from './visualizers/token-awarded-visualizer/token-awarded-visualizer.component';
import { GuardHitVisualizerComponent } from './visualizers/guard-hit-visualizer/guard-hit-visualizer.component';
import { GuardMissVisualizerComponent } from './visualizers/guard-miss-visualizer/guard-miss-visualizer.component';
import { RoundStartVisualizerComponent } from './visualizers/round-start-visualizer/round-start-visualizer.component';
import { TurnStartVisualizerComponent } from './visualizers/turn-start-visualizer/turn-start-visualizer.component';
import { CardPlayedVisualizerComponent } from './visualizers/card-played-visualizer/card-played-visualizer.component';

@Component({
  selector: 'app-game-log',
  standalone: true,
  imports: [
    CommonModule,
    PriestEffectVisualizerComponent,
    HandmaidProtectionVisualizerComponent,
    EffectFizzledVisualizerComponent,
    PlayerEliminatedVisualizerComponent,
    BaronCompareVisualizerComponent,
    PrinceDiscardVisualizerComponent,
    KingTradeVisualizerComponent,
    CountessDiscardVisualizerComponent,
    RoundEndVisualizerComponent,
    TokenAwardedVisualizerComponent,
    GuardHitVisualizerComponent,
    GuardMissVisualizerComponent,
    RoundStartVisualizerComponent,
    TurnStartVisualizerComponent,
    CardPlayedVisualizerComponent
  ],
  templateUrl: './game-log.component.html',
  styleUrls: ['./game-log.component.scss'],
  providers: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GameLogComponent implements AfterViewChecked {
  private datePipe = inject(DatePipe);

  @ViewChild('logContainer') private logContainer!: ElementRef;
  private readonly gameStateService = inject(GameStateService);

  readonly gameLogs: Signal<GameLogEntryDto[]>;

  constructor() {
    // Effect to scroll to bottom when gameLogs change
    effect(() => {
      const l =this.gameLogs(); // Access the signal to trigger the effect on change
      this.scrollToBottom();
    });
    this.gameLogs = this.gameStateService.gameLogs;
  }

  ngAfterViewChecked(): void {
    // Initial scroll to bottom after view is checked
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      if (this.logContainer?.nativeElement) {
        this.logContainer.nativeElement.scrollTop = this.logContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      console.error('Error scrolling log container:', err);
    }
  }

  formatLogMessage(log: GameLogEntryDto): string {
    let message = `[${this.datePipe.transform(log.timestamp, 'HH:mm:ss')}] `;
    const actingPlayerName = log.actingPlayerName === 'Game' ? 'Game' : `Player ${log.actingPlayerName}`;
    const targetPlayerName = log.targetPlayerName ? `Player ${log.targetPlayerName}` : '';
    const revealedCardName = log.revealedCardType ? CARD_DETAILS_MAP[log.revealedCardType]?.name : '';

    if (log.message) {
      // For generic messages, try to replace placeholders if any
      message += log.message
        .replace('{actingPlayerName}', actingPlayerName)
        .replace('{targetPlayerName}', targetPlayerName)
        .replace('{revealedCardName}', revealedCardName ?? '');
      return message;
    }

    // Default formatting based on event type (can be expanded)
    switch (log.eventType) {
      case 1: // PriestEffect
        message += `${actingPlayerName} used Priest and saw ${targetPlayerName}'s ${revealedCardName}.`;
        break;
      case 2: // HandmaidProtection
        message += `${actingPlayerName} is protected by Handmaid.`;
        break;
      // ... add more cases for other event types as needed ...
      default:
        message += `${actingPlayerName} triggered ${log.eventTypeName}.`;
        if (targetPlayerName) message += ` Target: ${targetPlayerName}.`;
        if (revealedCardName) message += ` Card: ${revealedCardName}.`;
        break;
    }
    return message;
  }

  trackByLogId(index: number, logEntry: GameLogEntryDto): string {
    return logEntry.id;
  }
}
