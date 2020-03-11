import { IOpenConnection } from '../hub/IOpenConnection';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

export class CommonStateModel {
    private readonly stateChangedObservable: Observable<ICommonStateChanged>;
    public readonly StateId: Observable<string>;
    constructor(private connection: IOpenConnection) {
        this.stateChangedObservable = connection.register<ICommonStateChanged>('changed');

        this.StateId = this.stateChangedObservable.pipe(
            map(s => s.StateId),
            shareReplay(1)
        );
        this.StateId.subscribe();
     }

    onCommonStateChanged(onCommonStateChanged: ICommonStateChanged) {
        console.log(`change received ${onCommonStateChanged.StateId}`);
        console.log(onCommonStateChanged);
    }
}

export interface ICommonStateChanged {
    readonly StateId: string;
    [key: string]: any;
}
