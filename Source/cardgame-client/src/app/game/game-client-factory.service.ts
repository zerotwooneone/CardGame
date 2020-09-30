import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GameClient } from './game-client';

@Injectable({
  providedIn: 'root'
})
export class GameClientFactoryService {

  constructor(private httpClient: HttpClient) { }

  create(gameId: string): GameClient {
    return new GameClient(gameId, this.httpClient);
  }
}


