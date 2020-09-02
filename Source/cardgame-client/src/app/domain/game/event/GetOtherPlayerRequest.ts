import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';

export class GetOtherPlayerRequest {
    constructor(readonly gameId: GameId,
                readonly player: PlayerId) {}
}
