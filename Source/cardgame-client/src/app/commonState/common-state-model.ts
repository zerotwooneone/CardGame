import { IOpenConnection } from '../hub/IOpenConnection';
import { Observable } from 'rxjs';
import { property } from 'src/pipes/property';

export class CommonStateModel {
    private readonly stateChangedObservable: Observable<ICommonStateChanged>;
    public readonly StateId: Observable<string>;
    public readonly DrawCount: Observable<number>;
    constructor(private connection: IOpenConnection) {
        this.stateChangedObservable = connection.register<ICommonStateChanged>('changed');

        this.StateId = this.stateChangedObservable.pipe(
            property(m => m.StateId)
        );

        this.DrawCount = this.stateChangedObservable.pipe(
            property(m => m.DrawCount)
        );
     }

    onCommonStateChanged(onCommonStateChanged: ICommonStateChanged) {
        console.log(`change received ${onCommonStateChanged.StateId}`);
        console.log(onCommonStateChanged);
    }
}

export interface ICommonStateChanged {
    readonly StateId: string;
    readonly DrawCount: number;
    [key: string]: any;
}
