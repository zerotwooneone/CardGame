import { StringUtil } from '../value/string-util';
import { Guid } from '../id/guid';

export class BusEvent<T> {
    protected constructor(readonly id: string,
                          readonly value?: T,
                          readonly correlationId?: string) { }
    public static Factory<T>(value?: T,
                             id?: string,
                             correlationId?: string) {
        const uid = StringUtil.IsNullOrWhiteSpace(id as string)
            ? Guid.newGuid()
            : id as string;
        return new BusEvent(uid, value, correlationId ?? uid);
    }
}


