import { Component, OnInit, Input, Inject } from '@angular/core';
import { Observable, of, forkJoin } from 'rxjs';
import { CurrentPlayerModel } from 'src/app/currentPlayer/current-player-model';
import { CurrentPlayerModelFactoryService } from 'src/app/currentPlayer/current-player-model-factory.service';
import { CommonStateFactoryService } from 'src/app/commonState/common-state-factory.service';
import { CommonStateModel, ICardId } from 'src/app/commonState/common-state-model';
import { withLatestFrom, map, tap, switchMap, concatMap, filter } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { GameClientFactoryService } from 'src/app/game/game-client-factory.service';

@Component({
  selector: 'cgc-game-board',
  templateUrl: './game-board.component.html',
  styleUrls: ['./game-board.component.scss']
})
export class GameBoardComponent implements OnInit {

  private _gameId: string;
  @Input()
  get gameId(): string { return this._gameId; }
  set gameId(gameId: string) {
    if (!!gameId && !!(gameId.trim())) {
      this.connect(gameId);
    }
  }
  otherPlayers: Observable<IOtherPlayer[]>;
  public currentPlayer: CurrentPlayerModel;
  private commonState: CommonStateModel;
  drawCount: number;
  discardTop: ICardId | null;
  discardCount: number;
  constructor(private readonly currentPlayerModelFactory: CurrentPlayerModelFactoryService,
    private readonly commonStateFactory: CommonStateFactoryService,
    private readonly gameClientFactory: GameClientFactoryService) { }

  async ngOnInit(): Promise<void> {
  }
  findNotCached(cachedKeys: string[], allKeys: string[]): string[] {
    return allKeys.filter(a => !cachedKeys.some(c => c === a));
  }

  public trackByPlayerId(player: any): string {
    return player.Id;
  }

  showDiscard1(): boolean {
    return this.discardCount >= 1;
  }
  showDiscard2(): boolean {
    return this.discardCount >= 2;
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
  async connect(gameId: string) {
    this.commonState = await this.commonStateFactory.get(gameId);
    this._gameId = gameId;

    const gameClient = this.gameClientFactory.create(gameId);
    const playerId = '9b644228-6c7e-4caa-becf-89e093ee299f';

    this.currentPlayer = await this.currentPlayerModelFactory
      .getById(playerId, gameClient, this.commonState);
    this.commonState
      .Turn
      .subscribe(t => this.currentPlayer.refresh());
    this.commonState
      .DrawCount
      .subscribe(v => this.drawCount = v);

    this.commonState
      .Discard
      .subscribe(array => {
        this.discardCount = array.length;
        this.discardTop = array.length ? array[array.length - 1] : null;
      });

    const playerInfos = this.commonState
      .PlayerIds;

    this.otherPlayers = this.commonState
      .PlayersInRound
      .pipe(
        withLatestFrom(playerInfos),
        map(([playersInRound, playerInfoArray]) => {
          const inRound = new Map(playersInRound.map((i): [string, string] => [i, i]));

          // todo: fix play info
          return playerInfoArray
            .filter(s => s !== playerId)
            .map(p => {
            const result: IOtherPlayer = {
              Id: p,
              name: p,
              isInRound: true,
            };
            return result;
          });
        }),
        property(m => m)
      );
  }

}

export interface IOtherPlayer {
  readonly Id: string;
  readonly name: string;
  readonly isInRound: boolean;
}


