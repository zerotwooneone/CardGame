import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';

export interface EliminateRequested {
    readonly gameId: GameId;
    readonly player: PlayerId;
}
