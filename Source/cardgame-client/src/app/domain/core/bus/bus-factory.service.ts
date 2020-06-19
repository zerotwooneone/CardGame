import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { CommonEvent, EventPayload } from './common-event';
import { BusEvent } from './bus-event';
import { Guid } from '../id/guid';
import { map, filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class BusFactoryService {

  private readonly commonBus: Subject<CommonEvent>;
  constructor() {
    this.commonBus = new Subject<CommonEvent>();
   }

  public createTransmitter<T extends EventPayload>(name: string): Subject<BusEvent<T>> {
    const subject = new Subject<BusEvent<T>>();

    subject.subscribe(e => {
      const commonEvent = CommonEvent.Factory(e.id, name, typeof e, e.value);
      if (commonEvent.success) {
        this.commonBus.next(commonEvent.value as CommonEvent);
      } else {
        const failedSubject = 'Failed to Transmit';
        const failedErrorMessage = failedSubject;
        const err = new Error(failedErrorMessage);
        const errorEvent = CommonEvent.Factory(Guid.newGuid(), failedSubject, typeof err, { error: err, subject: name });
        this.commonBus.next(errorEvent.value as CommonEvent);
      }
    });
    return subject;
  }

  public createReciever<T extends EventPayload>(name: string): Observable<BusEvent<T>> {
    const commonObservable = this.commonBus.asObservable();
    const result = commonObservable.pipe(
      filter(c => c.subject === name),
      map(c => BusEvent.Factory<T>(c.payload as T))
    );
    return result;
  }
}
