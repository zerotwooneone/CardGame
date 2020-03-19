import { Component, OnInit, Input } from '@angular/core';
import { Observable, of, forkJoin } from 'rxjs';
import { CurrentPlayerModel } from 'src/app/currentPlayer/current-player-model';
import { CurrentPlayerModelFactoryService } from 'src/app/currentPlayer/current-player-model-factory.service';
import { CommonStateFactoryService } from 'src/app/commonState/common-state-factory.service';
import { CommonStateModel, ICard } from 'src/app/commonState/common-state-model';
import { withLatestFrom, map, tap, switchMap, concatMap } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { PlayerService, PlayerCache, IPlayerInfo } from 'src/app/player/player.service';

@Component({
  selector: 'cgc-game-board',
  templateUrl: './game-board.component.html',
  styleUrls: ['./game-board.component.scss']
})
export class GameBoardComponent implements OnInit {

  @Input()
  public gameId: string;
  otherPlayers: Observable<IOtherPlayer[]>;
  public currentPlayer: Observable<CurrentPlayerModel>;
  private commonState: CommonStateModel;
  drawCount: number;
  discardTop: ICard | null;
  discardCount: number;
  private readonly playerCache: PlayerCache = {};
  constructor(private readonly currentPlayerModelFactory: CurrentPlayerModelFactoryService,
              private readonly commonStateFactory: CommonStateFactoryService,
              private readonly playerService: PlayerService) { }

  async ngOnInit(): Promise<void> {
    const currentPlayerId = 'some player id';
    this.currentPlayer = this.currentPlayerModelFactory
      .getById(currentPlayerId);

    this.commonState = await this.commonStateFactory.get(this.gameId);
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
      .PlayerIds
      .pipe(
        map(allPlayerIds => {
          const cachedPlayerIds = Object.keys(this.playerCache);
          const idsToQuery = this.findNotCached(cachedPlayerIds, allPlayerIds);
          if (idsToQuery.length) {
            this.playerService.updatePlayerCache(this.playerCache, this.gameId, ...idsToQuery);
          }
          const obs: Observable<IPlayerInfo>[] = [];
          const result = allPlayerIds.reduce((obsArray, id) => {
            obsArray.push(this.playerCache[id]);
            return obsArray;
          }, obs);
          return result;
        }),
        concatMap(a => forkJoin(a))
      );

    this.otherPlayers = this.commonState
      .PlayersInRound
      .pipe(
        withLatestFrom(playerInfos),
        map(([playersInRound, playerInfoArray]) => {
          const inRound = new Map(playersInRound.map((i): [string, string] => [i, i]));

          return playerInfoArray.map(p => {
            const result: IOtherPlayer = {
              Id: p.id,
              name: p.name,
              isInRound: inRound.has(p.id)
            };
            return result;
          });
        }),
        property(m => m)
      );
  }
  findNotCached(cachedKeys: string[], allKeys: string[]): string[] {
    return allKeys.filter(a => !cachedKeys.some(c => c === a));
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

export interface IOtherPlayer {
  Id: string;
  name: string;
  isInRound: boolean;
}


