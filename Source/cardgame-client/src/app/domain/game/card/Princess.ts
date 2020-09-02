import { EntityResult } from '../../core/entity/entity-result';
import { Card, PlayContext, DiscardContext } from './card';

export class Princess extends Card {
    protected async PlayImplementation(playContext: PlayContext): Promise<any> {
        return await this.Discard(playContext);
    }

    public async Discard(discardContext: DiscardContext): Promise<any> {
        await discardContext.eliminate(discardContext.player);
    }
}


