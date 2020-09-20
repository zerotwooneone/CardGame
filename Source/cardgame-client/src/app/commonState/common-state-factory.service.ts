import { Injectable } from '@angular/core';
import { OpenConnectionFactoryService } from '../hub/open-connection-factory.service';
import { CommonStateModel, ICommonStateChanged } from './common-state-model';
import { IOpenConnection } from '../hub/IOpenConnection';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CommonStateFactoryService {

  private previous: IAttempt;
  constructor(private openConnectionFactory: OpenConnectionFactoryService) { }

  async create(gameId: string): Promise<CommonStateModel> {
    const result = this.createNew(gameId);
    this.previous = {id: gameId, promise: result};
    return await result;
  }

  private async createNew(gameId: string): Promise<CommonStateModel> {
    const connection = await this.openConnectionFactory.open('https://localhost:44379/commonState');
    // const connection: IOpenConnection = {
    //   register: <TResult>(methodName: string) => {
    //     switch (methodName) {
    //       case 'changed':
    //         const changed: ICommonStateChanged =  {
    //           StateId: 'dummy state id',
    //           DrawCount: 4,
    //           PlayerIds: ['1', '2', '3'],
    //           Discard: [{Id: 'some card id', Value: 11},
    //             { Id: 'some other card id', Value: 12 }],
    //           CurrentPlayerId: '',
    //           PlayersInRound: ['1', '2']
    //         };
    //         return of(changed as unknown as TResult);
    //         break;
    //       default:
    //         throw new Error('unknow method name in register');
    //     }
    //   },
    //   send: (methodName: string, data: any) => Promise.reject('not set up to send')
    // };
    const result = new CommonStateModel(connection);
    return result;
  }

  async get(gameId: string): Promise<CommonStateModel> {
    return this.previous && this.previous.id === gameId
      ? this.previous.promise
      : this.create(gameId);
  }
}

interface IAttempt {
  readonly id: string;
  promise: Promise<CommonStateModel>;
}
