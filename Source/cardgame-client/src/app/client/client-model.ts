import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

export class ClientModel {
    readonly stateObservable: Observable<IStateChanged>;
    constructor(readonly Id: string,
                private events: IClientConnection) {
        this.stateObservable = this.events.StateChange
            .pipe(shareReplay(1));
        this.stateObservable.subscribe(); // kick off the replay
     }
    get State(): Observable<IStateChanged> {
        return this.stateObservable;
    }
}

export interface IStateChanged {
    readonly IsOpen: boolean;
}

export interface IClientConnection {
    readonly StateChange: Observable<IStateChanged>;
    close(): void;
}
