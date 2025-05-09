import { Component, ChangeDetectionStrategy, inject, Signal, ElementRef, ViewChild, AfterViewChecked, effect } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { GameStateService } from '../../services/game-state.service';
import { GameLogEntryDto } from '../../../../core/models/gameLogEntryDto';
import { CARD_DETAILS_MAP } from '../card/CARD_DETAILS_MAP'; // For card names

@Component({
  selector: 'app-game-log',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './game-log.component.html',
  styleUrls: ['./game-log.component.scss'],
  providers: [DatePipe], // Add DatePipe to providers
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GameLogComponent implements AfterViewChecked {
  private gameStateService = inject(GameStateService);
  private datePipe = inject(DatePipe);

  @ViewChild('logContainer') private logContainer!: ElementRef;

  gameLogs: Signal<GameLogEntryDto[]> = this.gameStateService.gameLogs;

  constructor() {
    // Effect to scroll to bottom when gameLogs change
    effect(() => {
      this.gameLogs(); // Access the signal to trigger the effect on change
      this.scrollToBottom();
    });
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
