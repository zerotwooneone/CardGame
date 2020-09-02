import { StringUtil } from './StringUtil';
import { EntityResult } from '../entity/entity-result';

export class CommonEvent {
    protected constructor(readonly id: string,
                          readonly type: string,
                          readonly subject: string,
                          readonly payload?: EventPayload,
                          readonly correlationId?: string) {}
    public static Factory(id: string,
                          subject: string,
                          type: string,
                          payload?: EventPayload,
                          correlationId?: string): EntityResult<CommonEvent> {
        if (StringUtil.IsNullOrWhiteSpace(id)) {
            EntityResult.CreateError('event id must be provided');
        }
        if (StringUtil.IsNullOrWhiteSpace(type)) {
            EntityResult.CreateError('event id must be provided');
        }
        return EntityResult.CreateSuccess(new CommonEvent(id, type, subject, payload, correlationId));
    }

}

export interface EventPayload {
    readonly [id: string]: {}|[];
    readonly eventId: string;
    readonly correlationId: string;
    readonly type: string;
}


