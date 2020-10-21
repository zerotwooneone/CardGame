import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CardModel } from '../card-model';
import { MatDialog } from '@angular/material/dialog';
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

  ngOnInit(): void {
  }

  onPlayClick(): void {
    const otherPlayerIds = this.otherPlayers
      .filter(o => o.isInRound)
      .map(o => o.Id);
    const targetPlayers = this.isTargetRequired()
      ? this.canTargetSelf() ? [this.playerId, ...otherPlayerIds] : otherPlayerIds
      : null;
    const targetRequired = this.canTargetSelf() || (otherPlayerIds.length > 1);
    const strengthRequired = CardStrength.Guard === this.card?.cardStrength;
    if (strengthRequired || targetRequired) {
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
    } else {
      const t = targetPlayers ?? [];
      this.playClick.emit({
        strength: null,
        target: t.length > 1 ? null : t[0],
      });
    }
  }

  private isTargetRequired(): boolean {
    switch (this.card?.cardStrength) {
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

  private canTargetSelf(): boolean {
    switch (this.card?.cardStrength) {
      case CardStrength.Prince:
        return true;
      case CardStrength.Guard:
      case CardStrength.Priest:
      case CardStrength.Baron:
      case CardStrength.King:
      case CardStrength.Handmaid:
      case CardStrength.Countess:
      case CardStrength.Princess:
      default:
        return false;
    }
  }

}
