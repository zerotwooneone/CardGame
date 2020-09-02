import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';

export class TradeHandsRequest {
    constructor(readonly player1: PlayerId,
                readonly player2: PlayerId,
                readonly gameId: GameId) {}
}


