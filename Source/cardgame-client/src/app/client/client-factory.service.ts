import { Injectable } from '@angular/core';
import { ClientModel, IStateChanged } from './client-model';
import { OpenConnectionFactoryService } from '../hub/open-connection-factory.service';
import { Subject } from 'rxjs';
import { BusFactoryService } from '../domain/core/bus/bus-factory.service';
import { TopicTokens } from '../domain/core/bus/topic-tokens';

@Injectable({
  providedIn: 'root'
})
export class ClientFactoryService {

  constructor(private openConnectionFactory: OpenConnectionFactoryService,
              private busFactoryService: BusFactoryService) { }

  async Create(clientId: IClientId): Promise<ClientModel> {
    const stateSubject = new Subject<IStateChanged>();
    const onClose = () => {
      stateSubject.next({IsOpen: false});
    };
    const events = {
      StateChange: stateSubject,
      close: () => { throw new Error('Not implemented'); }
    };

    const connection = await this.openConnectionFactory.open('https://localhost:5001/client', onClose);
    const connectResult = await connection.send<IClientConnected>('Connect', clientId);
    connection.register<ClientEvent>('OnClientEvent')
      .subscribe(clientEvent => {
        this.busFactoryService.publish<ClientEvent>(TopicTokens.clientEvent, clientEvent, clientEvent.correlationId, clientEvent.eventId);
      });
    console.log(`connection result:`, connectResult);

    const result = new ClientModel(connectResult.PlayerId, events);

    stateSubject.next({IsOpen: true});

    return result;
  }
}

export interface IClientId {
  readonly GameId: string;
}

export interface IClientConnected {
  readonly PlayerId: string;
}

export class ClientEvent {
  readonly eventId: string;
  readonly correlationId: string;
  readonly topic: string;
  readonly type: string;
  readonly data: {};
  readonly gameId: string;
}
