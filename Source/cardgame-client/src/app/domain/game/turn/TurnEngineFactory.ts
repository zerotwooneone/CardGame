import { GameId } from '../game/game-id';
import { TurnEngine } from './turn-engine';
import { EntityResult } from '../../core/entity/entity-result';

export interface TurnEngineFactory {
  create(gameId: GameId): EntityResult<TurnEngine>;
}

export class DefaultTurnEngineFactory implements TurnEngineFactory {
    create(gameId: GameId): EntityResult<TurnEngine> {
        return EntityResult.CreateSuccess(TurnEngine.Factory(gameId));
    }

}
