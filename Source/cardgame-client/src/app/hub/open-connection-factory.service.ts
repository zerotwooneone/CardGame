import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { IOpenConnection, RegisterCallback } from './IOpenConnection';

@Injectable({
  providedIn: 'root'
})
export class OpenConnectionFactoryService {

  constructor() { }

  public async open(url: string): Promise<IOpenConnection> {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .build();

    await hubConnection
        .start()
        .then(() => console.log(`Connection started ${url}`))
        .catch(err => console.error(`error while connecting ${url}`));

    return new OpenConnection(hubConnection);
  }
}

class OpenConnection implements IOpenConnection {
  public constructor(private connection: signalR.HubConnection) {}
  register<TCallback>(methodName: string, callback: RegisterCallback<TCallback>): void {
    this.connection.on(methodName, callback);
  }

}


