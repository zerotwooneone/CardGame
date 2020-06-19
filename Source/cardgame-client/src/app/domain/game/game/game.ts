import { GameId } from './game-id';
import { PlayerId } from '../player/player-id';
import { EntityResult } from '../../core/entity/entity-result';
import { Player } from '../player/player';

export class Game {
    protected players: PlayerId[];
    protected constructor(readonly id: GameId,
                          players: Player[]) {
                              this.players = Object.assign([], players);
                          }

    public static Factory(id: GameId,
                          players: Player[]): EntityResult<Game> {
            if (!id) {
                return EntityResult.CreateError('Id cannot be null');
            }
            if (!players || !players.length) {
                return EntityResult.CreateError('Players cannot be null or empty');
            }
            return EntityResult.CreateSuccess(new Game(id, players));
        }
}


