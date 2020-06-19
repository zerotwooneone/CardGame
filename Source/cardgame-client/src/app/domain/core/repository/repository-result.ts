export class RepositoryResult<TResult, TId> {
    protected constructor(readonly success: boolean,
                          readonly value: TResult[] | null,
                          readonly errors: ErrorCollection<TId> | null = null,
                          readonly reason: string | null,
                          readonly exception: Error | null) { }

    public static CreateSuccess<TResult, TId>(successes: TResult[],
                                              errors: ErrorCollection<TId>|null = null): RepositoryResult<TResult, TId> {
        if ((!successes || !successes.length)
            && errors && errors.length) {
            return this.CreateError('no successes returned', errors);
        }
        const newValue = this.copy(successes);
        return new RepositoryResult(true, newValue, errors, null, null);
    }

    public static CreateError<TResult, TId>(reason: string,
                                            errors: ErrorCollection<TId> | null = null,
                                            exception: Error | null = null): RepositoryResult<TResult, TId> {
        return new RepositoryResult<TResult, TId>(false, null, errors, reason, exception);
    }

    private static copy(aObject: any): any {
        if (!aObject) {
            return aObject;
        }
        let v: any;
        const bObject: any = Array.isArray(aObject) ? [] : {};
        for (const k in aObject) {
            if (true) {
                v = aObject[k];
                bObject[k] = (typeof v === 'object') ? this.copy(v) : v;
            }
        }
        return bObject;
    }
}

export interface ErrorCollection<T> { [id: string]: { id: T, error: string, exception ?: Error|null }; }
