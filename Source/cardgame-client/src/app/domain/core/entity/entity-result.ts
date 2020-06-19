export class EntityResult<T> {
    protected constructor(readonly success: boolean,
                          readonly value: T | null,
                          readonly reason: string | null,
                          readonly exception: Error | null) { }

    public static CreateSuccess<T>(value: T): EntityResult<T> {
        return new EntityResult(true, value, null, null);
    }

    public static CreateError<T>(reason: string, exception: Error | null = null): EntityResult<T> {
        return new EntityResult<T>(false, null, reason, exception);
    }
}
