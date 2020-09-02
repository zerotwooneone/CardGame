import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';

export class DiscardAndDrawRequest {
    constructor(readonly gameId: GameId,
                readonly target: PlayerId) {}
}
