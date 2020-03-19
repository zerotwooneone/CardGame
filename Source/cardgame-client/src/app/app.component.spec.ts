import { Spectator, createComponentFactory } from '@ngneat/spectator';
import { RouterTestingModule } from '@angular/router/testing';
import { AppComponent } from './app.component';
import { GameBoardComponent } from './gameBoard/game-board/game-board.component';
import { MockComponent } from 'ng-mocks';

describe('AppComponent', () => {
  let spectator: Spectator<AppComponent>;
  const createComponent = createComponentFactory({
    component: AppComponent,
    declarations: [MockComponent(GameBoardComponent)],
    imports: [RouterTestingModule]
  });

  beforeEach(() => spectator = createComponent());

  it('should create the app', () => {
    const app = spectator.component;
    expect(app).toBeTruthy();
  });

  it(`should have as title 'cardgame-client'`, () => {
    const app = spectator.component;
    expect(app.title).toEqual('cardgame-client');
  });
});
