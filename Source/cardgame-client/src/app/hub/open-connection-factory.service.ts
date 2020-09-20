import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { HubConnectionWrapper } from './HubConnectionWrapper';

@Injectable({
  providedIn: 'root'
})
export class OpenConnectionFactoryService {

  constructor() { }

  public async open(url: string,
                    closeCallback: () => void = () => {}): Promise<HubConnectionWrapper> {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .build();

    hubConnection.onclose(e => {
      console.log(`Connection closed Error:${e}`);
      closeCallback();
    });

    await hubConnection
      .start()
      .then(() => console.log(`Connection started ${url}`))
      .catch(err => console.error(`error while connecting ${url}`));

    return new HubConnectionWrapper(hubConnection);
    //return Promise.reject('OpenConnectionFactoryService dummy call');
  }
}
