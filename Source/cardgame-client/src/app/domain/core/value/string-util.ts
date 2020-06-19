export class StringUtil {
    public static IsNullOrWhiteSpace(value: string) {
        return typeof value === 'undefined' ||
               value === null ||
               value.trim().length === 0;
    }
}
