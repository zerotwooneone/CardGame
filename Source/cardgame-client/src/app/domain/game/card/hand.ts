import { CardId } from './card-id';
import { ValueResult } from '../../core/value/value-result';
import { CardStrength } from './CardStrength';
import { CompareToResult } from './CompareToResult';

export class Hand {
    public static Factory(a: CardId, b?: CardId): ValueResult<Hand> {
        if (!a) {
            return ValueResult.CreateError<Hand>(`at least one card id must be provided`);
        }
        return ValueResult.CreateSuccess<Hand>(new Hand(a, b));
    }
    protected constructor(readonly a: CardId, readonly b?: CardId) { }

    public Contains(...types: CardStrength[]): boolean {
        if (!types || !types.length) { return false; }
        for (const type of types) {
            const found = [this.a, this.b].find(c => !!c && c.HasType(type));
            if (!!found) {
                return true;
            }
        }
        return false;
    }

    public Equals(other: Hand): boolean {
        return (!!other) && other.a.Equals(this.a) &&
        (other.b === this.b || (other.b?.Equals(this.b) ?? true));
    }
    compareTo(other: Hand): CompareToResult {
        if (!!this.b || !!other.b) { throw new Error('cannot compare hands with multiple values'); }
        return this.a.compareStrength(other.a);
    }
}