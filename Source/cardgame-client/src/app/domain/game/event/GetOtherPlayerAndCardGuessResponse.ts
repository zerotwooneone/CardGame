import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';
import { CardStrength } from '../card/CardStrength';

export class GetOtherPlayerAndCardGuessResponse {
    constructor(readonly gameId: GameId,
                readonly target: PlayerId,
                readonly strength: CardStrength) {}
}
