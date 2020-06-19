
export class StringUtil {
    public static IsNullOrWhiteSpace(input: string): boolean {
        return (typeof input === 'undefined' ||
            input === null ||
            !input.trim().length);
    }
}
