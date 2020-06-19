import { ValueResult } from '../../core/value/value-result';
import { StringUtil } from '../../core/value/string-util';

export class GameId {
    public static Factory(value: string): ValueResult<GameId> {
        if (StringUtil.IsNullOrWhiteSpace(value)) {
            return ValueResult.CreateError<GameId>('value cannot be null');
        }
        const minValueLength = 16;
        if (value.length < minValueLength) {
            return ValueResult.CreateError<GameId>(`value must be at least ${minValueLength} characters long`);
        }
        return ValueResult.CreateSuccess<GameId>(new GameId(value));
    }
    protected constructor(readonly value: string) {}

    public Equals(other: GameId): boolean {
        return (!!other) && other.value === this.value;
    }
}
