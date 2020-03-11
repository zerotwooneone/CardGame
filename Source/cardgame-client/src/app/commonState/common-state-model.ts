import { IOpenConnection } from '../hub/IOpenConnection';
import { Observable } from 'rxjs';

export class CommonStateModel {
    private readonly stateChangedObservable: Observable<ICommonStateChanged>;
    constructor(private connection: IOpenConnection) {
        this.stateChangedObservable = connection.register<ICommonStateChanged>('changed');
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
