import { Component, OnInit, Input } from '@angular/core';
import { IOtherPlayer } from 'src/app/gameBoard/game-board/game-board.component';

@Component({
  selector: 'cgc-other-player',
  templateUrl: './other-player.component.html',
  styleUrls: ['./other-player.component.scss']
})
export class OtherPlayerComponent implements OnInit {

  @Input() player: IOtherPlayer;
  constructor() { }

  ngOnInit(): void {
  }

}


