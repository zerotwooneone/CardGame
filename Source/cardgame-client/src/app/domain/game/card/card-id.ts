import { ValueResult } from '../../core/value/value-result';
import { CardValue } from './card-value';
import { CardStrength } from './CardStrength';
import { CompareToResult } from './CompareToResult';

export class CardId {
    public static Factory(value: CardValue, varient: number): ValueResult<CardId> {
        const varientMin = 1;
        const varientMaxByValue: {[id: number]: number} = {
            1: 5,
            2: 2,
            3: 2,
            4: 2,
            5: 2,
            6: 1,
            7: 1,
            8: 1
        };
        const varientMax = varientMaxByValue[value.value];
        if (varient < varientMin ||
            varient > varientMax) {
            return ValueResult.CreateError<CardId>(
                `varient must be between ${varientMin} and ${varientMax} inclusive for value ${value.value}`);
        }
        return ValueResult.CreateSuccess<CardId>(new CardId(value, varient));
    }
    protected constructor(readonly value: CardValue, readonly varient: number) { }

    public Equals(other?: CardId): boolean {
        return (!!other) && other.value === this.value;
    }
    HasType(type: CardStrength): boolean {
        return this.value.value === type;
    }
    compareStrength(other: CardId): CompareToResult {
        if (!other) { return CompareToResult.SourceGreaterThan; }
        return this.value.compareStrength(other.value);
    }
}
