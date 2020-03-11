import { IOpenConnection } from './IOpenConnection';
import { Observable, bindCallback, Subject } from 'rxjs';
export class HubConnectionWrapper implements IOpenConnection {
    async send<TResponse>(methodName: string, data: any): Promise<TResponse> {
        return await this.connection.invoke(methodName, data);
    }
    public constructor(private connection: signalR.HubConnection) { }
    innerRegister<TCallback>(methodName: string, callback: RegisterCallback<TCallback>): void {
        this.connection.on(methodName, callback);
    }
    register<TResult>(methodName: string): Observable<TResult> {
        const subject = new Subject<TResult>();
        const func: RegisterCallback<TResult> = data => subject.next(data);
        this.innerRegister(methodName, func);
        return subject.asObservable();
    }
}

export type RegisterCallback<T>= (data: T) => void;
