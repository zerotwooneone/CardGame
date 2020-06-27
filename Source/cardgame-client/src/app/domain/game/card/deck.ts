import { CardId } from './card-id';
import { ValueResult } from '../../core/value/value-result';

export class Deck {
    protected static get MaxDeckLength(): number {
        return 16;
    }
    public static Factory(discard: readonly CardId[],
                          drawCount: number): ValueResult<Deck> {
        if (drawCount < 0) {
            return ValueResult.CreateError('draw count must be greater than or equal to zero');
        }
        if (drawCount + discard.length > this.MaxDeckLength) {
            return ValueResult.CreateError(`The draw and discard piles must not exceed ${this.MaxDeckLength}`);
        }
        return ValueResult.CreateSuccess(new Deck(discard.slice(), drawCount));
    }
    protected constructor(readonly discard: readonly CardId[],
                          readonly drawCount: number) {}

    public Draw(): ValueResult<Deck> {
        const deck = Deck.Factory(this.discard, this.drawCount - 1);
        if (!deck.success) {
            return ValueResult.CreateError(`error creating deck: ${deck.reason}`, deck.exception);
        }
        return ValueResult.CreateSuccess(deck.value as Deck);
    }

    public Discard(card: CardId): ValueResult<Deck> {
        if (this.discard.length >= Deck.MaxDeckLength) {
            return ValueResult.CreateError('the discard pile is full');
        }
        const discard = this.discard.slice();
        discard.push(card);
        return ValueResult.CreateSuccess(new Deck(discard, this.drawCount));
    }
}
