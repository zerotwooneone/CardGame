import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { CurrentPlayerModel } from 'src/app/currentPlayer/current-player-model';
import { CurrentPlayerModelFactoryService } from 'src/app/currentPlayer/current-player-model-factory.service';
import { CommonStateFactoryService } from 'src/app/commonState/common-state-factory.service';
import { CommonStateModel, ICard } from 'src/app/commonState/common-state-model';
import { property } from 'src/pipes/property';

@Component({
  selector: 'cgc-game-board',
  templateUrl: './game-board.component.html',
  styleUrls: ['./game-board.component.scss']
})
export class GameBoardComponent implements OnInit {

  constructor(private readonly currentPlayerModelFactory: CurrentPlayerModelFactoryService,
              private readonly commonStateFactory: CommonStateFactoryService) { }

  public readonly otherPlayers: Observable<any[]> = of([
    { Id: '1', name: 'player 1', isInRound: true },
    { Id: '2', name: 'player 2', isInRound: true },
    { Id: '3', name: 'player 3', isInRound: false }]);
  public currentPlayer: Observable<CurrentPlayerModel>;
  private commonState: CommonStateModel;
  drawCount: number;
  discardTop: ICard | null;
  discardCount: number;

  async ngOnInit(): Promise<void> {
    const currentPlayerId = 'some player id';
    this.currentPlayer = this.currentPlayerModelFactory
      .getById(currentPlayerId);

    const gameId = 'some game id';
    this.commonState = await this.commonStateFactory.get(gameId);

    this.commonState
      .DrawCount
      .subscribe(v => this.drawCount = v);

    this.commonState
      .Discard
      .subscribe(array => {
        this.discardCount = array.length;
        this.discardTop = array.length ? array[array.length - 1] : null;
      });
  }

  public trackByPlayerId(player: any): string {
    return player.Id;
  }

  showDiscard1Contents(): boolean {
    return this.discardCount === 1;
  }
  showDiscard1(): boolean {
    return this.discardCount >= 1;
  }

  showDiscard2Contents(): boolean {
    return this.discardCount === 2;
  }
  showDiscard2(): boolean {
    return this.discardCount >= 2;
  }

  showDiscard3Contents(): boolean {
    return this.discardCount > 2;
  }
  showDiscard3(): boolean {
    return this.discardCount > 2;
  }

  showDraw1Contents(): boolean {
    return this.drawCount === 1;
  }
  showDraw1(): boolean {
    return this.drawCount >= 1;
  }

  showDraw2Contents(): boolean {
    return this.drawCount === 2;
  }
  showDraw2(): boolean {
    return this.drawCount >= 2;
  }

  showDraw3Contents(): boolean {
    return this.drawCount > 2;
  }
  showDraw3(): boolean {
    return this.drawCount > 2;
  }

}


