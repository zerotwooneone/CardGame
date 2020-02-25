
export type RegisterCallback<T>= (data: T) => void;

export interface IOpenConnection {
  register<TCallback>(methodName: string, callback: RegisterCallback<TCallback>): void;
  send(methodName: string, data: any): Promise<any>;
}
