import { Component, OnInit, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { CurrentPlayerModel } from 'src/app/currentPlayer/current-player-model';
import { withLatestFrom, map } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { GameModel } from 'src/app/game/game-model';
import { ICardId } from 'src/app/commonState/common-state-model';

@Component({
  selector: 'cgc-game-board',
  templateUrl: './game-board.component.html',
  styleUrls: ['./game-board.component.scss']
})
export class GameBoardComponent implements OnInit {

  @Input()
  gameModel: GameModel;
  @Input()
  currentPlayer: CurrentPlayerModel;
  otherPlayers: Observable<IOtherPlayer[]>;
  drawCount: number;
  discardTop: ICardId | null;
  discardCount: number;
  constructor() { }

  async ngOnInit(): Promise<void> {
    const playerId = '9b644228-6c7e-4caa-becf-89e093ee299f';

    this.gameModel
      .Turn
      .subscribe(t => this.currentPlayer.refresh());
    this.gameModel
      .DrawCount
      .subscribe(v => this.drawCount = v);

    this.gameModel
      .Discard
      .subscribe(array => {
        this.discardCount = array.length;
        this.discardTop = array.length ? array[array.length - 1] : null;
      });

    this.otherPlayers = this.gameModel
      .PlayersInRound
      .pipe(
        withLatestFrom(this.gameModel.PlayerIds),
        map(([playersInRound, allPlayerIds]) => {
          const inRound = new Map(playersInRound.map((i): [string, string] => [i, i]));
          // todo: fix play info
          return allPlayerIds
            .filter(s => s !== playerId)
            .map((p, i) => {
              const result: IOtherPlayer = {
                Id: p,
                name: p,
                isInRound: inRound.has(p),
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
}

export interface IOtherPlayer {
  readonly Id: string;
  readonly name: string;
  readonly isInRound: boolean;
}


