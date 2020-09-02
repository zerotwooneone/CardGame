import { Card } from './card';
import { CardStrength } from './CardStrength';

export class Countess extends Card {
    public GetMustPlay(other?: CardStrength): boolean {
        if (!other) { return false; }
        return other === CardStrength.King || other === CardStrength.Prince;
    }
}
