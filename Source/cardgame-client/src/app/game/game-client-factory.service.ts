import { Injectable } from '@angular/core';
import { HttpClient } from '@aspnet/signalr';
import { GameClient } from './GameClient';

@Injectable({
  providedIn: 'root'
})
export class GameClientFactoryService {

  constructor(private httpClient: HttpClient) { }

  create(gameId: string): GameClient {
    return new GameClient(gameId, this.httpClient);
  }
}


