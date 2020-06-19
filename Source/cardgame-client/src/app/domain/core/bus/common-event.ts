import { StringUtil } from './StringUtil';
import { EntityResult } from '../entity/entity-result';

export class CommonEvent {
    protected constructor(readonly id: string,
                          readonly type: string,
                          readonly subject: string,
                          readonly payload?: EventPayload) {}
    public static Factory(id: string,
                          subject: string,
                          type: string,
                          payload?: EventPayload): EntityResult<CommonEvent> {
        if (StringUtil.IsNullOrWhiteSpace(id)) {
            EntityResult.CreateError('event id must be provided');
        }
        if (StringUtil.IsNullOrWhiteSpace(type)) {
            EntityResult.CreateError('event id must be provided');
        }
        return EntityResult.CreateSuccess(new CommonEvent(id, type, subject, payload));
    }

}

export interface EventPayload {
    [id: string]: {}|[];
}


