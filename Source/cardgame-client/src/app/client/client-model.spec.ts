import { ClientModel, IClientConnection, IStateChanged } from './client-model';
import { Observable, Subject } from 'rxjs';

class MockClientConnection implements IClientConnection {
  StateChange: Observable<IStateChanged> = new Subject<IStateChanged>();
  close(): void {
    throw new Error('Method not implemented.');
  }


}

describe('ClientModel', () => {
  it('should create an instance', () => {
    expect(new ClientModel('id', new MockClientConnection())).toBeTruthy();
  });
});
