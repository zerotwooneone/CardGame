import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ICardId } from 'src/app/commonState/common-state-model';
import { IOtherPlayer } from 'src/app/gameBoard/game-board/game-board.component';

@Component({
  selector: 'cgc-other-player',
  templateUrl: './other-player.component.html',
  styleUrls: ['./other-player.component.scss']
})
export class OtherPlayerComponent implements OnInit {

  @Input() player: IOtherPlayer;
  @Input() revealedCard: ICardId | null;
  constructor() { }

  ngOnInit(): void {
  }
}


