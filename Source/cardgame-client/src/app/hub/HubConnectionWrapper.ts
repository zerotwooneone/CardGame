import { IOpenConnection } from './IOpenConnection';
export class HubConnectionWrapper implements IOpenConnection {
    async send<TResponse>(methodName: string, data: any): Promise<TResponse> {
        return await this.connection.invoke(methodName, data);
    }
    public constructor(private connection: signalR.HubConnection) { }
    register<TCallback>(methodName: string, callback: RegisterCallback<TCallback>): void {
        this.connection.on(methodName, callback);
    }
}

export type RegisterCallback<T>= (data: T) => void;
