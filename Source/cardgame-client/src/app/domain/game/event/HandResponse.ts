import { GameId } from '../game/game-id';
import { CardId } from '../card/card-id';

export class HandResponse {
    constructor(readonly gameId: GameId,
                readonly card: CardId) {}
}
