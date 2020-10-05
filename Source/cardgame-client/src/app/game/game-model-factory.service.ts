import { Injectable } from '@angular/core';
import { CommonStateModel } from '../commonState/common-state-model';
import { GameClientFactoryService } from './game-client-factory.service';
import { GameModel } from './game-model';

@Injectable({
  providedIn: 'root'
})
export class GameModelFactoryService {

  constructor(private readonly gameClientFactory: GameClientFactoryService) { }

  public async create(commonModel: CommonStateModel,
                      gameId: string): Promise<GameModel> {
    const gameClient = this.gameClientFactory.create(gameId);
    const gameState = await gameClient.getCommonState().toPromise();
    return new GameModel(commonModel, gameState);
  }
}
