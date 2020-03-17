import { Spectator, createComponentFactory } from '@ngneat/spectator';

import { GameBoardComponent } from './game-board.component';
import { CommonStateFactoryService } from 'src/app/commonState/common-state-factory.service';
import { CommonStateModel } from 'src/app/commonState/common-state-model';
import { Subject, BehaviorSubject } from 'rxjs';
import { MockComponent } from 'ng-mocks';
import { OtherPlayerComponent } from 'src/app/otherPlayer/other-player/other-player.component';
import { CurrentPlayerComponent } from 'src/app/currentPlayer/current-player/current-player.component';
import { testproperty } from 'src/pipes/testproperty';

describe('GameBoardComponent', () => {
  let spectator: Spectator<GameBoardComponent>;
  const createComponent = createComponentFactory({
    component: GameBoardComponent,
    mocks: [
      CommonStateFactoryService,
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
    },
    detectChanges: false
  }));

  it('should create', () => {
    spectator.detectChanges();
    expect(spectator.component).toBeTruthy();
  });

  describe('otherPlayers', () => {
    it('should have players', async () => {
      const commonStateFactory = spectator.get(CommonStateFactoryService, true);
      const commonStateModel = spectator.get(CommonStateModel, true);
      addPlayerIds(commonStateModel, '1', '2');
      addPlayersInRound(commonStateModel, '1');
      commonStateFactory.get
        .withArgs(gameId)
        .and
        .returnValue(Promise.resolve(commonStateModel));
      spectator.detectChanges();

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

