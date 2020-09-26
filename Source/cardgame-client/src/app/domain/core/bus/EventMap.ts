import { EventPayload } from './common-event';
import { TopicTokens } from './topic-tokens';
import { ClientEvent } from 'src/app/client/client-factory.service';
import { CommonGameStateChanged } from 'src/app/commonState/CommonGameStateChanged';

export class EventMap {
  private static readonly mappers: { [token: string]: MapDefinition; } = {
    [TopicTokens.clientEvent]: EventMap.GetJsonMapDefinition(typeof ClientEvent),
    [TopicTokens.GameStateChanged]: EventMap.GetJsonMapDefinition(typeof CommonGameStateChanged),
  };
  static GetJsonMapDefinition(type: string): MapDefinition {
    return {
      type,
      receiveMap: (t, ep) => {
        const x = ((ep as EventPayload).JSON as { value: string });
        return JSON.parse(x.value);
      },
      requestMap: (e, eid, cid) => ({
        type,
        correlationId: cid,
        eventId: eid,
        JSON: { value: JSON.stringify(e) }
      } as EventPayload)
    };
  }
  public Receive<T>(token: string, type: string, event?: EventPayload): T {
    const mapper = EventMap.mappers[token];
    if (!mapper) {
      // todo: do something
    }
    return mapper.receiveMap?.(type, event) as T;
  }
  public Transmit<T>(event: T,
                     token: string,
                     eventId: string,
                     correlationId: string): EventPayload | undefined {
    const mapper = EventMap.mappers[token];
    if (!mapper) {
      // todo: do something
    }
    return mapper.requestMap?.(event, eventId, correlationId);
  }
}

export interface MapDefinition {
    receiveMap?: (type: string, e?: EventPayload) => {};
    requestMap?: (e: {}, eventId?: string, correlationId?: string) => EventPayload;
    type: string;
}
