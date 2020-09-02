import { Card, PlayContext } from './card';

export class Priest extends Card {
    protected async PlayImplementation(playContext: PlayContext): Promise<any> {
        const target = await playContext.getOtherPlayer(playContext.player);

        if (!target.aborted) {
            const targetCard = await playContext.getHand(target.value);
            // we dont use this here; this UI will display this
        }
        return super.PlayImplementation(playContext);
    }
 }
