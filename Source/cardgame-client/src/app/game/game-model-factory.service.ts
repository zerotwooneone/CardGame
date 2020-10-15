import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { CardRevealed } from '../commonState/CardRevealed';
import { CommonStateModel } from '../commonState/common-state-model';
import { BusFactoryService } from '../domain/core/bus/bus-factory.service';
import { TopicTokens } from '../domain/core/bus/topic-tokens';
import { GameClientFactoryService } from './game-client-factory.service';
import { GameModel } from './game-model';

@Injectable({
  providedIn: 'root'
})
export class GameModelFactoryService {

  constructor(private readonly gameClientFactory: GameClientFactoryService,
    private readonly bus: BusFactoryService) { }

  public async create(commonModel: CommonStateModel,
    gameId: string): Promise<GameModel> {
    const gameClient = this.gameClientFactory.create(gameId);
    const gameState = await gameClient.getCommonState().toPromise();

    // todo: consider player model, dispose of subscription
    const cardRevealedSubject = new Subject<CardRevealed>();
    const s = this.bus.registerToReceive<CardRevealed>(TopicTokens.CardRevealed, cr => cardRevealedSubject.next(cr));

    return new GameModel(commonModel, gameState, cardRevealedSubject.asObservable());
  }
}
