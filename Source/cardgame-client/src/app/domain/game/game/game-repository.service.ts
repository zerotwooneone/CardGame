import { Injectable } from '@angular/core';
import { GameId } from './game-id';
import { Game } from './game';
import { RepositoryResult, ErrorCollection } from '../../core/repository/repository-result';
import { Player } from '../player/player';
import { PlayerRepositoryService, TestIds as PlayerTestIds } from '../player/player-repository.service';

@Injectable({
  providedIn: 'root'
})
export class GameRepositoryService {

  constructor(protected readonly playerRepository: PlayerRepositoryService) { }

  public GetById(id: GameId): RepositoryResult<Game, GameId> {
    if (!id) {
      return RepositoryResult.CreateError('id not provided');
    }
    if (id.Equals(TestIds.NotFound)) {
      return RepositoryResult.CreateError('not found');
    }
    // todo: retreive a real game
    const playerId = PlayerTestIds.Dummy;
    const player = this.playerRepository.GetByIds([playerId]);
    if (!player.success) {
      return RepositoryResult.CreateError(`PlayerRepository Error:${player.reason}`, player.errors, player.exception);
    }
    const result = Game.Factory(id, player.value as Player[]);
    if (result.success) {
      return RepositoryResult.CreateSuccess([result.value as Game]);
    }
    const errors: ErrorCollection<GameId> = {};
    errors[id.value] = {
      id,
      error: result.reason as string,
      exception: result.exception};
    return RepositoryResult.CreateError(`Game.Factory Error:${result.reason}`, errors);
  }
}

export class TestIds {
  public static NotFound: GameId = GameId.Factory('This will return not found').value as any;
}
