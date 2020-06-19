import { ValueResult } from '../../core/value/value-result';

export class Score {
    public static readonly Default: Score = new Score(0);
    public static Factory(value: number): ValueResult<Score> {
        const minValue = 0;
        if (value < minValue) {
            return ValueResult.CreateError<Score>(`value must be at least ${minValue}`);
        }
        const maxValue = 20;
        if (value > maxValue) {
            return ValueResult.CreateError<Score>(`value must be less than ${maxValue}`);
        }
        return ValueResult.CreateSuccess<Score>(new Score(value));
    }

    protected constructor(readonly value: number) { }

    public GetIncremented(): Score {
        const result = Score.Factory(this.value + 1);
        if (result.success) {
            return result.value as any;
        }
        throw new Error(result.reason as any);
    }

    public Equals(other: Score): boolean {
        return (!!other) && other.value === this.value;
    }
}
