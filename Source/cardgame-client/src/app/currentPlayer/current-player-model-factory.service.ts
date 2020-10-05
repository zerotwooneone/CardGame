import { Injectable } from '@angular/core';
import { CurrentPlayerModel } from './current-player-model';
import { GameClientFactoryService } from '../game/game-client-factory.service';
import { GameModel } from '../game/game-model';

@Injectable({
  providedIn: 'root'
})
export class CurrentPlayerModelFactoryService {
  constructor(private readonly gameClientFactory: GameClientFactoryService) { }

  async getById(currentPlayerId: string,
    gameId: string,
    gameModel: GameModel): Promise<CurrentPlayerModel> {
    const gameClient = this.gameClientFactory.create(gameId);
    const playerObservable = gameClient.getPlayer(currentPlayerId);
    return new CurrentPlayerModel(currentPlayerId,
      playerObservable.toPromise(),
      gameClient,
      gameModel);
  }
}
