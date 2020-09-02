import { Card, PlayContext } from './card';

export class King extends Card {
    protected async PlayImplementation(playContext: PlayContext): Promise<any> {
        const target = await playContext.getOtherPlayer(playContext.player);
        if (!target.aborted) {
            await playContext.tradeHands(playContext.player, target.value);
        }
    }
}
