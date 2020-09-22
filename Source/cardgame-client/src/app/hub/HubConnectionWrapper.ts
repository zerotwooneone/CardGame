import { IOpenConnection } from './IOpenConnection';
import { Observable, bindCallback, Subject } from 'rxjs';
import { take } from 'rxjs/operators';
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
        const func: RegisterCallback<TResult> = data => {
            //console.warn(methodName, data);
            subject.asObservable().pipe(take(1)).subscribe(d => console.warn(methodName, d));
            return subject.next(data);
        };
        this.innerRegister(methodName, func);
        return subject.asObservable();
    }
}

export type RegisterCallback<T>= (data: T) => void;
