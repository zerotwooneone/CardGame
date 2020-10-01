import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CardModel } from '../card-model';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ChoiceOutput, PlayChoiceComponent, ChoiceInput } from '../play-choice/play-choice.component';
import { CardStrength } from 'src/app/domain/card/CardStrength';
import { IOtherPlayer } from 'src/app/gameBoard/game-board/game-board.component';

@Component({
  selector: 'cgc-playable-card',
  templateUrl: './playable-card.component.html',
  styleUrls: ['./playable-card.component.scss']
})
export class PlayableCardComponent implements OnInit {

  @Input()
  card: CardModel;
  @Input()
  isTurn: boolean;
  @Input()
  playerId: string;
  @Input()
  otherPlayers: readonly IOtherPlayer[];
  @Output()
  playClick = new EventEmitter<ChoiceOutput>();

  constructor(private matDialog: MatDialog) { }

  get value(): number {
    return this.card?.value;
  }

  ngOnInit(): void {
  }

  onPlayClick(): void {
    const otherPlayers = this.otherPlayers.filter(o => o.isInRound).map(o => o.Id);
    const targetPlayers = this.isPlayerRequired()
      ? [this.playerId, ...otherPlayers]
      : null;
    const strengthRequired = CardStrength.Guard === this.card?.value;
    // todo handle not targeting self
    if (!strengthRequired && (!targetPlayers || !targetPlayers.length)) {
      this.playClick.emit({
        strength: null,
        target: null,
      });
    } else {
      const playDetails: ChoiceInput = {
        strengthRequired,
        targetPlayers
      };
      const ref = this.matDialog.open(PlayChoiceComponent, {
        data: playDetails,
      });
      ref.afterClosed().subscribe((result: ChoiceOutput | null) => {
        if (!!result) {
          this.playClick.emit(result);
        }
      });
    }
  }

  isPlayerRequired(): boolean {
    switch (this.card?.value) {
      case CardStrength.Guard:
      case CardStrength.Priest:
      case CardStrength.Baron:
      case CardStrength.Prince:
      case CardStrength.King:
        return true;
      case CardStrength.Handmaid:
      case CardStrength.Countess:
      case CardStrength.Princess:
      default:
        return false;
    }
  }

}
