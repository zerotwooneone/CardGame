import { CommonStateModel, ICommonStateChanged } from './common-state-model';
import { IOpenConnection } from '../hub/IOpenConnection';
import { Observable, Subject } from 'rxjs';
import { timeout, take } from 'rxjs/operators';
import { testproperty } from 'src/pipes/testproperty';

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

  it('should have most recent stateId', async () => {
    const mockConnection = new MockOpenConnection();
    const expected = 'test';
    const model = new CommonStateModel(mockConnection);

    mockConnection.registerSubject.next({StateId: expected});
    const actual = await model.StateId.pipe(
      testproperty).toPromise();

    expect(expected).toEqual(actual);
  });
});
