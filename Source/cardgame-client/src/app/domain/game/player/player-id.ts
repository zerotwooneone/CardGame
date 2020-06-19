import { ValueResult } from '../../core/value/value-result';
import { StringUtil } from '../../core/value/string-util';

export class PlayerId {
    public static Factory(value: string): ValueResult<PlayerId> {
        if (StringUtil.IsNullOrWhiteSpace(value)) {
            return ValueResult.CreateError<PlayerId>('value cannot be null');
        }
        const minValueLength = 16;
        if (value.length < minValueLength) {
            return ValueResult.CreateError<PlayerId>(`value must be at least ${minValueLength} characters long`);
        }
        return ValueResult.CreateSuccess<PlayerId>(new PlayerId(value));
    }
    protected constructor(readonly value: string) { }

    public Equals(other: PlayerId): boolean {
        return (!!other) && other.value === this.value;
    }
}
