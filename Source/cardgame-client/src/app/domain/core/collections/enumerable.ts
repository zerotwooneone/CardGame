export class Enumerable<T> {
    constructor(private values: T[]) { }

    public ToArray(): T[] {
        return Object.assign([], this.values);
    }
}
