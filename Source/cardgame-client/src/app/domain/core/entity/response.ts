export class Response<T> {
    protected constructor(readonly aborted: boolean,
                          readonly value: T | null,
                          readonly reason?: string) { }

    public static CreateSuccess<T>(value: T): Response<T> {
        return new Response(true, value);
    }

    public static CreateAborted<T>(reason?: string): Response<T> {
        return new Response<T>(false, null, reason);
    }
}
