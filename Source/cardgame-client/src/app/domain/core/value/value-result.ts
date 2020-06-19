export class ValueResult<T> {
    protected constructor(readonly success: boolean,
                          readonly value: T|null,
                          readonly reason: string|null,
                          readonly exception: Error|null) {}

    public static CreateSuccess<T>(value: T): ValueResult<T> {
        return new ValueResult(true, value, null, null);
    }

    public static CreateError<T>(reason: string, exception: Error|null = null): ValueResult<T> {
        return new ValueResult<T>(false, null, reason, exception);
    }
}
