import { Spectator, createComponentFactory } from '@ngneat/spectator';

import { GameBoardComponent } from './game-board.component';
import { CommonStateFactoryService } from 'src/app/commonState/common-state-factory.service';
import { CommonStateModel } from 'src/app/commonState/common-state-model';
import { Subject, BehaviorSubject, EMPTY } from 'rxjs';
import { MockComponent } from 'ng-mocks';
import { OtherPlayerComponent } from 'src/app/otherPlayer/other-player/other-player.component';
import { CurrentPlayerComponent } from 'src/app/currentPlayer/current-player/current-player.component';
import { testproperty } from 'src/pipes/testproperty';
import { CurrentPlayerModelFactoryService } from 'src/app/currentPlayer/current-player-model-factory.service';
import { isNumber } from 'util';

describe('GameBoardComponent', () => {
  let spectator: Spectator<GameBoardComponent>;
  const createComponent = createComponentFactory({
    component: GameBoardComponent,
    mocks: [
      CommonStateFactoryService,
      CurrentPlayerModelFactoryService,
      CommonStateModel
    ],
    declarations: [
      MockComponent(OtherPlayerComponent),
      MockComponent(CurrentPlayerComponent)
    ]
  });
  const gameId = 'some game id';
  beforeEach(() => spectator = createComponent({
    props: {
      gameId
    }
  }));

  it('should create', () => {
    expect(spectator.component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should create otherPlayers', async () => {
      const commonStateFactory = spectator.get(CommonStateFactoryService, true);
      const commonStateModel = spectator.get(CommonStateModel, true);
      addPlayerIds(commonStateModel, '1', '2');
      addPlayersInRound(commonStateModel, '1');
      addDrawCount(commonStateModel);
      addDiscard(commonStateModel);
      commonStateFactory.get
        .withArgs(gameId)
        .and
        .returnValue(Promise.resolve(commonStateModel));

      await spectator.component.ngOnInit();

      const players = await spectator.component
        .otherPlayers
        .pipe(testproperty)
        .toPromise();

      expect(players).toContain(jasmine.objectContaining({Id: '1', isInRound: true}));
      expect(players).toContain(jasmine.objectContaining({ Id: '2', isInRound: false }));
    });
  });
});

function addPlayerIds(commonStateModel: CommonStateModel, ...ids: string[]): Subject<string[]> {
  const a = commonStateModel as any;
  const result = !!ids && ids.length
    ? new BehaviorSubject<string[]>(ids)
    : new Subject<string[]>();
  a.PlayerIds = result;
  return result;
}

function addPlayersInRound(commonStateModel: CommonStateModel, ...ids: string[]): Subject<string[]> {
  const a = commonStateModel as any;
  const result = !!ids && ids.length
    ? new BehaviorSubject<string[]>(ids)
    : new Subject<string[]>();
  a.PlayersInRound = result;
  return result;
}

function addDrawCount(commonStateModel: CommonStateModel, initalValue: number = -1): Subject<number> {
  const drawCount = initalValue >= 0
    ? new BehaviorSubject<number>(initalValue)
    : new Subject<number>();
  (commonStateModel as any).DrawCount = drawCount;
  return drawCount;
}

function addDiscard(commonStateModel: CommonStateModel, initalValue: number = -1): Subject<number> {
  const subject = initalValue >= 0
    ? new BehaviorSubject<number>(initalValue)
    : new Subject<number>();
  (commonStateModel as any).Discard = subject;
  return subject;
}


