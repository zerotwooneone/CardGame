import { Injectable } from '@angular/core';
import { BusFactoryService } from '../../core/bus/bus-factory.service';
import { TurnRepositoryService } from './turn-repository.service';
import { PlayerId } from '../player/player-id';
import { GameId } from '../game/game-id';
import { TurnEngine } from './turn-engine';
import { CardStrength } from '../card/CardStrength';
import { TurnEngineFactory } from './TurnEngineFactory';
import { IAppService } from '../../core/service/IAppService';
import { TopicTokens } from '../../core/bus/topic-tokens';
import { CardId } from '../card/card-id';

@Injectable({
  providedIn: 'root'
})
export class TurnService implements IAppService {
  constructor(protected readonly busFactory: BusFactoryService,
              readonly turnRepository: TurnRepositoryService,
              readonly turnEngineFactory: TurnEngineFactory) { }

  public Initialize(): void {}
  public async Start(gameId: GameId,
                     player: PlayerId): Promise<any> {

    const subscription = this.busFactory.registerToReceive<CardPlayed>(TopicTokens.cardPlayedToken, this.onCardPlayed);
    // todo: unsubscribe if turn is cancelled () => subscription.unsubscribe()

    const engineResult = this.turnEngineFactory.create(gameId);
    if (!engineResult.success) {
      throw new Error(`could not create engine: ${engineResult.reason}`);
    }
    const engine = engineResult.value as TurnEngine;

    const setResult = this.turnRepository.Set(engine);
    if (!setResult.Success) {
      throw new Error('could not set the turn');
    }
    await engine.Start({
      requestDraw: (game, p) => this.requestDraw(game, p, correlationId)
      });

    await this.busFactory.publish<TurnStartedEvent>(TopicTokens.turnStarted,
      {
        gameId,
        player
      },
      correlationId);
  }

  protected async requestEndTurn(gameId: GameId, player: PlayerId, correlationId: string): Promise<any> {
    await this.busFactory.publish<TurnCompletedEvent>(TopicTokens.turnCompleted,
      { gameId, player },
      correlationId);
  }
  protected async requestDraw(gameId: GameId, player: PlayerId, correlationId: string): Promise<any> {
    await this.busFactory.publish<CardDrawRequested>(TopicTokens.cardDrawRequest,
      { gameId, player, correlationId },
      correlationId);
  }

  protected async onCardPlayed(event: CardPlayed, correlationId: string): Promise<any> {
    const engine = this.turnRepository.Get(event.gameId);
    if (!engine.Success) {
      // todo: report missing turn
    }
    await (engine.value as TurnEngine).Play({
      endTurn: (game, player) => this.requestEndTurn(game, player, correlationId)
    });
  }
}

export interface CardPlayed {
  readonly gameId: GameId;
  readonly card: CardId;
  readonly player: PlayerId;
  readonly targetPlayer: PlayerId;
  readonly targetCard: CardId;
  readonly targetStrength: CardStrength;
}

export interface TurnStartedEvent {
  gameId: GameId;
  player: PlayerId;
}

export interface TurnCompletedEvent {
  gameId: GameId;
  player: PlayerId;
}
