import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { IPlayer } from 'src/app/otherPlayer/other-player/IPlayer';
import { CurrentPlayerModel } from 'src/app/currentPlayer/current-player-model';
import { CurrentPlayerModelFactoryService } from 'src/app/currentPlayer/current-player-model-factory.service';

@Component({
  selector: 'cgc-game-board',
  templateUrl: './game-board.component.html',
  styleUrls: ['./game-board.component.scss']
})
export class GameBoardComponent implements OnInit {

  constructor(private currentPlayerModelFactory: CurrentPlayerModelFactoryService) { }

  public readonly otherPlayers: Observable<IPlayer[]> = of([{ Id: '1' }, { Id: '2' }]);
  public currentPlayer: Observable<CurrentPlayerModel>;

  ngOnInit(): void {
    const currentPlayerId = 'some player id';
    this.currentPlayer = this.currentPlayerModelFactory
      .getById(currentPlayerId);
  }

  public trackByPlayerId(player: IPlayer): string {
    return player.Id;
  }

}


