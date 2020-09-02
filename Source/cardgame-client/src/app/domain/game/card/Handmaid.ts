import { Card, PlayContext } from './card';

export class Handmaid extends Card {
    protected async PlayImplementation(playContext: PlayContext): Promise<any> {
        await playContext.protect(playContext.player);
    }
}
