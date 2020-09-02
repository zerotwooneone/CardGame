import { Card, PlayContext } from './card';
import { CompareToResult } from './CompareToResult';
import { PlayerId } from '../player/player-id';

export class Baron extends Card {
    protected async PlayImplementation(playContext: PlayContext): Promise<any> {
        const target = await playContext.getOtherPlayer(playContext.player);

        if (!target.aborted) {
            const targetCard = await playContext.getHand(target.value as PlayerId);
            const comparison = this.id.compareStrength(targetCard);
            if (comparison === CompareToResult.SourceGreaterThan) {
                await playContext.eliminate(target.value as PlayerId);
            } else if (comparison === CompareToResult.SourceLessThan) {
                await playContext.eliminate(playContext.player);
            }
        }
    }
}
