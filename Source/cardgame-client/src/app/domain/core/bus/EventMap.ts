import { EventPayload } from './common-event';
import { TopicTokens } from './topic-tokens';
import { ClientEvent } from 'src/app/client/ClientEvent';
import { CommonGameStateChanged } from 'src/app/commonState/CommonGameStateChanged';
import { CardRevealed } from 'src/app/commonState/CardRevealed';
import { CardPlayed } from 'src/app/commonState/CardPlayed';

export class EventMap {
  private static readonly mappers: { [token: string]: MapDefinition; } = {
    [TopicTokens.clientEvent]: EventMap.GetJsonMapDefinition(typeof ClientEvent),
    [TopicTokens.GameStateChanged]: EventMap.GetJsonMapDefinition(typeof CommonGameStateChanged),
    [TopicTokens.CardRevealed]: EventMap.GetJsonMapDefinition(typeof CardRevealed),
    [TopicTokens.CardPlayed]: EventMap.GetJsonMapDefinition(typeof CardPlayed),
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
      console.error(`cannot find map for ${token}`);
      return undefined;
    }
    return mapper.requestMap?.(event, eventId, correlationId);
  }
}

export interface MapDefinition {
    receiveMap?: (type: string, e?: EventPayload) => {};
    requestMap?: (e: {}, eventId?: string, correlationId?: string) => EventPayload;
    type: string;
}
