import { CommonStateModel } from './common-state-model';
import { IOpenConnection } from '../hub/IOpenConnection';

class MockOpenConnection implements IOpenConnection {
  register<TCallback>(methodName: string, callback: import('../hub/IOpenConnection').RegisterCallback<TCallback>): void { }
  send(methodName: string, data: any): Promise<any> { return Promise.resolve(true); }
}

describe('CommonStateModel', () => {
  it('should create an instance', () => {
    expect(new CommonStateModel(new MockOpenConnection())).toBeTruthy();
  });
});
