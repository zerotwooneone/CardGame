import { Player } from '../player/player';
import { Card, UpdatedState } from '../card/card';
import { Round } from '../Round/round';
import { Deck } from '../card/deck';
import { GameId } from '../game/game-id';
import { PlayerId } from '../player/player-id';
import { CardStrength } from '../card/CardStrength';
import { Hand } from '../card/hand';

export class TurnEngine {

    protected hasPlayed = false;

    protected constructor(readonly gameId: GameId,
                          readonly player: PlayerId) {}
    public static Factory(gameId: GameId,
                          player: PlayerId): TurnEngine {
        return new TurnEngine(gameId, player);
    }

    async Start(turnStartContext: TurnStartContext): Promise<any> {
        await turnStartContext.requestDraw(this.gameId, this.player);
    }

    public async Play(playContext: PlayContext): Promise<any> {
        if (this.hasPlayed) {
            throw Error('can only play once per turn');
        }
        this.hasPlayed = true;
        await playContext.endTurn(this.gameId, this.player);
    }
}

interface TurnStartContext {
    requestDraw(game: GameId, player: PlayerId): Promise<any>;
}

interface PlayContext {
    endTurn(game: GameId, player: PlayerId): Promise<any>;
}

