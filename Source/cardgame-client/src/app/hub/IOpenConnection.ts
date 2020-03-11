import { Observable } from 'rxjs';

export type RegisterCallback<T>= (data: T) => void;

export interface IOpenConnection {
  register<TResult>(methodName: string): Observable<TResult>;
  send(methodName: string, data: any): Promise<any>;
}
