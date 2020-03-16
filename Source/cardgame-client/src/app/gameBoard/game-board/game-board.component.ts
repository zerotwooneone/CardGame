import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { CurrentPlayerModel } from 'src/app/currentPlayer/current-player-model';
import { CurrentPlayerModelFactoryService } from 'src/app/currentPlayer/current-player-model-factory.service';

@Component({
  selector: 'cgc-game-board',
  templateUrl: './game-board.component.html',
  styleUrls: ['./game-board.component.scss']
})
export class GameBoardComponent implements OnInit {

  constructor(private currentPlayerModelFactory: CurrentPlayerModelFactoryService) { }

  public readonly otherPlayers: Observable<any[]> = of([
    { Id: '1', name: 'player 1', isInRound: true },
    { Id: '2', name: 'player 2', isInRound: true }]);
  public currentPlayer: Observable<CurrentPlayerModel>;

  ngOnInit(): void {
    const currentPlayerId = 'some player id';
    this.currentPlayer = this.currentPlayerModelFactory
      .getById(currentPlayerId);
  }

  public trackByPlayerId(player: any): string {
    return player.Id;
  }

}


