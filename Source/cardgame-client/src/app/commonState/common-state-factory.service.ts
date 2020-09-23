import { Injectable } from '@angular/core';
import { CommonStateModel } from './common-state-model';
import { of, Subject } from 'rxjs';
import { BusFactoryService } from '../domain/core/bus/bus-factory.service';
import { TopicTokens } from '../domain/core/bus/topic-tokens';
import { CommonGameStateChanged } from './CommonGameStateChanged';

@Injectable({
  providedIn: 'root'
})
export class CommonStateFactoryService {

  private previous: IAttempt;
  constructor(private bus: BusFactoryService) { }

  async create(gameId: string): Promise<CommonStateModel> {
    const result = this.createNew(gameId);
    this.previous = {id: gameId, promise: result};
    return await result;
  }

  private async createNew(gameId: string): Promise<CommonStateModel> {
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
    const eventSubject = new Subject<CommonGameStateChanged>();
    function OnCommonGameStateChanged(e: CommonGameStateChanged) {
      if (e.gameId === gameId) {
        eventSubject.next(e);
      }
    }
    // todo handle this subscription
    const stateObservable = this.bus
      .registerToReceive<CommonGameStateChanged>(TopicTokens.GameStateChanged, OnCommonGameStateChanged.bind(this));
    const result = new CommonStateModel(eventSubject.asObservable());
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


