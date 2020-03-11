import { IOpenConnection } from '../hub/IOpenConnection';
import { Observable } from 'rxjs';
import { property } from 'src/pipes/property';

export class CommonStateModel {
    private readonly stateChangedObservable: Observable<ICommonStateChanged>;
    public readonly StateId: Observable<string>;
    constructor(private connection: IOpenConnection) {
        this.stateChangedObservable = connection.register<ICommonStateChanged>('changed');

        this.StateId = this.stateChangedObservable.pipe(
            property(m => m.StateId)
        );
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
