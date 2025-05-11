import {Component, inject, Input, OnInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';
import {UiInteractionService} from '../../../../../../core/services/ui-interaction-service.service';

@Component({
  selector: 'app-priest-effect-visualizer',
  templateUrl: './priest-effect-visualizer.component.html',
  styleUrls: ['./priest-effect-visualizer.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    CardComponent
  ]
})
export class PriestEffectVisualizerComponent implements OnInit {
  @Input() logEntry!: GameLogEntryDto;

  public CardType = CardType;
  public canSeeRevealedCard: boolean = false;
  private uiInteractionService = inject(UiInteractionService);

  constructor() { }

  ngOnInit(): void {
    if (this.logEntry && this.logEntry.revealedCardValue !== undefined && this.logEntry.revealedCardValue !== null) {
      this.canSeeRevealedCard = true;
    } else {
      this.canSeeRevealedCard = false;
    }
  }

  onPriestCardInfoClicked(): void {
    this.uiInteractionService.requestScrollToCardReference(CardType.Priest);
  }

  onRevealedCardInfoClicked(): void {
    if (this.logEntry.revealedCardValue) {
      this.uiInteractionService.requestScrollToCardReference(this.logEntry.revealedCardValue);
    }
  }
}
