import { Component, OnInit, Input } from '@angular/core';
import { ConnectableObservable, merge, Observable } from 'rxjs';
import { CurrentPlayerModel } from 'src/app/currentPlayer/current-player-model';
import { withLatestFrom, map, shareReplay, distinctUntilChanged, switchMap, filter, publish, take } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { GameModel } from 'src/app/game/game-model';
import { ICardId } from 'src/app/commonState/common-state-model';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { ScoreCardComponent, ScoreCardInput } from '../score-card/score-card.component';
import { CommonKnowledgePlayer } from 'src/app/game/game-client';

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
  constructor(private bottomSheet: MatBottomSheet) { }

  async ngOnInit(): Promise<void> {
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
            .filter(s => s !== this.currentPlayer.Id)
            .map((p, i) => {
              const revealedCardObservable = this.createCardRevealedObservable(p);
              const result: IOtherPlayer = {
                Id: p,
                name: p,
                isInRound: inRound.has(p),
                revealedCardObservable: revealedCardObservable,
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

  createCardRevealedObservable(playerId: string): Observable<ICardId | null> {

    const currentPlayerIdObservable = (this.gameModel.CurrentPlayerId.pipe(publish()) as ConnectableObservable<string>)
      .refCount(); // we publish here so that all observers avoid the sharedReplay value
    currentPlayerIdObservable.pipe(take(5)).subscribe(); // subscribe to kick this off and clear out the cached value before unsubscribing

    const targetTurnObservable = this.gameModel.CardRevealedObservable.pipe(
      switchMap(cr => currentPlayerIdObservable.pipe(
        filter(t => t === cr.playerId)
      ))
    );
    const afterTargetTurnObservable = targetTurnObservable.pipe(
      switchMap(m => currentPlayerIdObservable)
    );

    const cardRevealedObs = this.gameModel.CardRevealedObservable.pipe(
      filter(e => e.targetId === playerId),
      map(e => {
        return ({
          strength: e.targetCardStrength,
          varient: e.targetCardVariant
        } as ICardId);
      })
    );
    const clearRevealedObservable = afterTargetTurnObservable.pipe(map(t => null as ICardId | null));
    const result = merge(
      cardRevealedObs.pipe(map(cr => cr as ICardId | null)),
      clearRevealedObservable).pipe(map(m => { console.warn("result", m); return m; }));
    return result;
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
  scoreClick(): void {
    const data: ScoreCardInput = {
      playerScores: this.gameModel.PlayerScores,
    };
    this.bottomSheet.open(ScoreCardComponent, { data });
  }
}

export interface IOtherPlayer {
  readonly Id: string;
  readonly name: string;
  readonly isInRound: boolean;
  readonly revealedCardObservable: Observable<ICardId | null>;
}



