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
    const subject = new Subject<IStateChanged>();
    const onClose = () => {
      subject.next({IsOpen: false});
    };

    const events = {
      StateChange: subject,
      close: () => { throw new Error('Not implemented'); }
    };
    const result = new ClientModel(events);

    const connection = await this.openConnectionFactory.open('https://localhost:44379/client', onClose);
    const connectResult = await connection.send('connect', clientId);

    console.log(`connection result: ${connectResult}`);
    console.log(connectResult);

    subject.next({IsOpen: true});

    return result;
  }
}

export interface IClientId {
  readonly Id: string;
}
