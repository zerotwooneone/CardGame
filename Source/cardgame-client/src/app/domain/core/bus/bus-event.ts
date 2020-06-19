import { EventPayload } from './common-event';
import { StringUtil } from '../value/string-util';
import { Guid } from '../id/guid';

export class BusEvent<T extends EventPayload> {
    protected constructor(readonly id: string,
                          readonly value?: T) { }
    public static Factory<T extends EventPayload>(value?: T,
                                                  id?: string) {
        const uid = StringUtil.IsNullOrWhiteSpace(id as string)
            ? Guid.newGuid()
            : id as string;
        return new BusEvent(uid, value);
    }
}
