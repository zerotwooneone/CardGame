import { ValueResult } from '../../core/value/value-result';
import { CardStrength } from './CardStrength';
import { CompareToResult } from './CompareToResult';

export class CardValue {
    public static Factory(value: CardStrength): ValueResult<CardValue> {
        const minValue = CardStrength.Min;
        const maxValue = CardStrength.Max;
        if (value < minValue ||
            value > maxValue) {
            return ValueResult.CreateError<CardValue>(`value must be between ${minValue} and ${maxValue} inclusive`);
        }
        return ValueResult.CreateSuccess<CardValue>(new CardValue(value));
    }
    protected constructor(readonly value: CardStrength) { }

    public Equals(other: CardValue): boolean {
        return (!!other) && other.value === this.value;
    }
    compareStrength(other: CardValue): CompareToResult {
        if (!other) { return CompareToResult.SourceGreaterThan; }
        if (this.value > other.value) {
            return CompareToResult.SourceGreaterThan;
        } else if (this.value === other.value) {
            return CompareToResult.Equals;
        } else {
            return CompareToResult.SourceLessThan;
        }
    }
}


