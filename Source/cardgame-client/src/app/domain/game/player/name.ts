import { ValueResult } from '../../core/value/value-result';
import { StringUtil } from '../../core/value/string-util';

export class Name {
    public static Factory(value: string): ValueResult<Name> {
        if (StringUtil.IsNullOrWhiteSpace(value)) {
            return ValueResult.CreateError<Name>('value cannot be null');
        }
        const minValueLength = 3;
        if (value.length < minValueLength) {
            return ValueResult.CreateError<Name>(`name must be at least ${minValueLength} characters long`);
        }
        const maxValueLength = 240;
        if (value.length > maxValueLength) {
            return ValueResult.CreateError<Name>(`name must be less than ${maxValueLength} characters long`);
        }
        return ValueResult.CreateSuccess<Name>(new Name(value));
    }
    protected constructor(readonly value: string) { }

    public Equals(other: Name): boolean {
        return (!!other) && other.value === this.value;
    }
}
