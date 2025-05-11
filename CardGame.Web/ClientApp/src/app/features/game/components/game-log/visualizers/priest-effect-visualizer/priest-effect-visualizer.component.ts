import {Component, Input, OnInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import {CardComponent} from '../../../card/card.component';
import {GameLogEntryDto} from '../../../../../../core/models/gameLogEntryDto';
import {CardType} from '../../../../../../core/models/cardType';

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

  constructor() { }

  ngOnInit(): void {
    if (this.logEntry && this.logEntry.revealedCardType !== undefined && this.logEntry.revealedCardType !== null) {
      this.canSeeRevealedCard = true;
    } else {
      this.canSeeRevealedCard = false;
    }
  }
}
