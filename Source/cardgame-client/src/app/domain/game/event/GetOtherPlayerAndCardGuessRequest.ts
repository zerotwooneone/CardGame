import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';

export class GetOtherPlayerAndCardGuessRequest {
    constructor(readonly gameId: GameId,
                readonly player: PlayerId) {}
}
