import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';

export class Protected {
    constructor(readonly gameId: GameId,
                readonly target: PlayerId) {}
}
