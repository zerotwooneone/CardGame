import { Injectable } from '@angular/core';
import { ClientModel, IStateChanged } from './client-model';
import { OpenConnectionFactoryService } from '../hub/open-connection-factory.service';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ClientFactoryService {

  constructor(private openConnectionFactory: OpenConnectionFactoryService) { }

  async Create(clientId: IClientId): Promise<ClientModel> {
    const stateSubject = new Subject<IStateChanged>();
    const onClose = () => {
      stateSubject.next({IsOpen: false});
    };
    const events = {
      StateChange: stateSubject,
      close: () => { throw new Error('Not implemented'); }
    };

    const connection = await this.openConnectionFactory.open('https://localhost:44379/client', onClose);
    const connectResult = await connection.send<IClientConnected>('Connect', clientId);
    console.log(`connection result: ${connectResult}`);
    console.log(connectResult);

    const result = new ClientModel(connectResult.Id, events);

    stateSubject.next({IsOpen: true});

    return result;
  }
}

export interface IClientId {
  readonly GameId: string;
}

export interface IClientConnected {
  readonly Id: string;
}
