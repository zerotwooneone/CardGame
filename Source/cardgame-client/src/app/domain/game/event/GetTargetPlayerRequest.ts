import { GameId } from '../game/game-id';

export class GetTargetPlayerRequest {
    constructor(readonly gameId: GameId) {}
}
