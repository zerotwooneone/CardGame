import { CommonStateModel, ICommonStateChanged } from './common-state-model';
import { IOpenConnection } from '../hub/IOpenConnection';
import { Observable, Subject } from 'rxjs';

class MockOpenConnection implements IOpenConnection {
  registerSubject: Subject<ICommonStateChanged> = new Subject<ICommonStateChanged>();
  register<TResult>(methodName: string): Observable<TResult> {
    return methodName === 'changed'
      ? this.registerSubject.asObservable() as any
      : undefined as unknown as Observable<TResult>;
    }
  send(methodName: string, data: any): Promise<any> { return Promise.resolve(true); }
}

describe('CommonStateModel', () => {
  it('should create an instance', () => {
    expect(new CommonStateModel(new MockOpenConnection())).toBeTruthy();
  });
});
