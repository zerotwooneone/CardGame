import { Injectable } from '@angular/core';
import { PlayerId } from './player-id';
import { Player } from './player';
import { RepositoryResult, ErrorCollection } from '../../core/repository/repository-result';
import { Name } from './name';

@Injectable({
  providedIn: 'root'
})
export class PlayerRepositoryService {

  constructor() { }

  public GetByIds(ids: PlayerId[]): RepositoryResult<Player, PlayerId> {
    if (!ids || !ids.length) {
      return RepositoryResult.CreateError('id not provided');
    }

    const errors: ErrorCollection<PlayerId> = {};
    if (ids.find(x => x.Equals(TestIds.NotFound))) {
      errors[TestIds.NotFound.value] = {
        id: TestIds.NotFound,
        error: 'not found'
      };
    }

    // todo: retreive a real entity
    const results: Player[] = [];
    for (const id of ids) {
      const playerName = Name.Factory('some name');
      if (playerName.success) {
        const result = Player.Factory(id, playerName.value as Name);
        if (result.success) {
          results.push(result.value as Player);
        } else {
          errors[id.value] = {
            id,
            error: 'error in player factory',
            exception: result.exception
          };
        }
      } else {
        errors['error in name factory'] = {
          id: TestIds.NotFound,
          error: 'error in name factory',
          exception: playerName.exception
        };
      }
    }
    return RepositoryResult.CreateSuccess(results, errors);
  }
}

export class TestIds {
  public static NotFound: PlayerId = PlayerId.Factory('This will return not found').value as PlayerId;
  public static Dummy: PlayerId = PlayerId.Factory('12345678901234567').value as PlayerId;
}

