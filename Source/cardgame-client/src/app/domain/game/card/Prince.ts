import { Card, PlayContext } from './card';
import { PlayerId } from '../player/player-id';

export class Prince extends Card {
    protected async PlayImplementation(playContext: PlayContext): Promise<any> {
        const discardTarget = await playContext.getTargetPlayer();
        if (!discardTarget.aborted) {
            await playContext.discardAndDraw(discardTarget.value as PlayerId);
        }
    }
}
