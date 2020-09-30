import { Injectable } from '@angular/core';
import { CurrentPlayerModel } from './current-player-model';
import { GameClient } from '../game/game-client';
import { CommonStateModel } from '../commonState/common-state-model';

@Injectable({
  providedIn: 'root'
})
export class CurrentPlayerModelFactoryService {
  constructor() { }

  async getById(currentPlayerId: string,
    gameClient: GameClient,
    commonState: CommonStateModel): Promise<CurrentPlayerModel> {
    const playerObservable = gameClient.getPlayer(currentPlayerId);
    return new CurrentPlayerModel(currentPlayerId,
      playerObservable.toPromise(),
      gameClient,
      commonState);
  }
}
