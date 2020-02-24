import { IOpenConnection, RegisterCallback } from '../hub/IOpenConnection';

export class CommonStateModel {
    constructor(private connection: IOpenConnection) {
        connection.register<ICommonStateChanged>('changed', this.onCommonStateChanged);
     }

    onCommonStateChanged(onCommonStateChanged: ICommonStateChanged) {
        console.log(`change received ${onCommonStateChanged.StateId}`);
        console.log(onCommonStateChanged);
    }
}

interface ICommonStateChanged {
    readonly StateId: string;
}
