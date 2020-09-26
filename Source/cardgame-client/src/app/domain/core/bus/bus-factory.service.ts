import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { CommonEvent, EventPayload } from './common-event';
import { map, filter, take, switchMap } from 'rxjs/operators';
import { BusSubscription } from './BusSubscription';
import { EventMap } from './EventMap';
import { Guid } from '../id/guid';

@Injectable({
  providedIn: 'root'
})
export class BusFactoryService {
  private readonly eventMap: EventMap;
  constructor() {
    this.eventMap = new EventMap();
    this.commonBus = new Subject<CommonEvent>();
   }

  private readonly commonBus: Subject<CommonEvent>;

  public publish<T>(token: string, value: T, correlationId?: string, eventId?: string): Promise<any> {
    const realEventId = eventId ?? 'bus factory, need to generate unique corr id'; // todo: generate unique id
    const realCorrelationId = correlationId ?? realEventId;
    const mappedEvent = this.eventMap.Transmit(value, token, realEventId, realCorrelationId);

    const commonEvent = CommonEvent.Factory(realEventId, token, typeof mappedEvent?.value, mappedEvent, realCorrelationId);
    if (commonEvent.success) {
      this.commonBus.next(commonEvent.value as CommonEvent);
    } else {
      const failedSubject = 'Failed to Transmit';
      const failedErrorMessage = commonEvent.reason ?? 'error occured creating common event, no reason given';
      const err = new Error(failedErrorMessage);
      const errorEventId = Guid.newGuid().toString();
      const errorEvent = CommonEvent.Factory(
        Guid.newGuid(),
        failedSubject,
        typeof err,
        {
          error: err,
          message: failedErrorMessage,
          subject: token,
          eventId: errorEventId,
          correlationId: errorEventId,
          type: typeof err
        },
        correlationId);
      this.commonBus.next(errorEvent.value as CommonEvent);
    }
    return Promise.resolve({message: 'this is a dummy object. we didnt actually wait for anything'});
  }

  public registerToReceive<T>(token: string, handler: (value: T, correlationId?: string) => any): BusSubscription {
    const obs = this.createObserver<T>(token);
    const sub = obs.subscribe(c => handler(c.value, c.correlationId));
    const result: BusSubscription = {
      add: (b: BusSubscription) => sub.add((b as any)._sub), // todo: this any cast is a hack
      unsubscribe: () => sub.unsubscribe
    };
    (result as any)._sub = sub;
    return result;
  }
  protected createObserver<T>(token: string): Observable<{value: T, id: string, correlationId: string | undefined}> {
    const commonObservable = this.commonBus.asObservable();
    // const rmapper = BusFactoryService.mappers[token];
    // if (!rmapper) {
    //   throw new Error(`no receiver for ${token}`); // todo: publish error
    // }
    const obs = commonObservable.pipe(
      filter(c => c.subject === token),
      map(c => {
        const event = this.eventMap.Receive(token, c.type, c.payload) as T;
        return { value: event, id: c.id, correlationId: c.correlationId};
      })
    );
    return obs;
  }

  public async awaitResponse<TTrasmit, TReceive>(
    requestToken: string,
    repsonseToken: string,
    event: TTrasmit,
    correlationId: string,
    eventId?: string): Promise<TReceive> {
      const realEventId = eventId ?? 'bus factory, need to generate unique corr id'; // todo: generate unique id
      const realCorrelationId = correlationId ?? realEventId;
      const reciever = this.createObserver<TReceive>(repsonseToken);
      const result = reciever.pipe(
        filter(e => e.correlationId === realCorrelationId),
        take(1),
        // todo: need to add a timeout
        map(e => e.value)
      );
      const publish = await this.publish<TTrasmit>(requestToken, event, realCorrelationId, realEventId);

      return result.toPromise();
    }
}






