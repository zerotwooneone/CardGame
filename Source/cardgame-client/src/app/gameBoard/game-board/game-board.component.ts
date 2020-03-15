import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { IPlayer } from 'src/app/otherPlayer/other-player/IPlayer';
import { ICurrentPlayer } from 'src/app/currentPlayer/current-player/ICurrentPlayer';

@Component({
  selector: 'cgc-game-board',
  templateUrl: './game-board.component.html',
  styleUrls: ['./game-board.component.scss']
})
export class GameBoardComponent implements OnInit {

  constructor() { }

  public readonly otherPlayers: Observable<IPlayer[]> = of([{ Id: '1' }, { Id: '2' }]);
  public readonly currentPlayer: ICurrentPlayer = { Id: 'some id', Name: 'some player name'};

  ngOnInit(): void {
  }

  public trackByPlayerId(player: IPlayer): string {
    return player.Id;
  }

}


