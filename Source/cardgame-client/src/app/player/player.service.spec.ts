import { TestBed } from '@angular/core/testing';
import { createServiceFactory, SpectatorService } from '@ngneat/spectator';

import { PlayerService, IPlayerInfo, PlayerCache, ApiPlayerInfo } from './player.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { EMPTY } from 'rxjs';

describe('PlayerService', () => {
  let spectator: SpectatorService<PlayerService>;
  let httpClient: HttpClient;
  let httpTestingController: HttpTestingController;
  const createService = createServiceFactory({
    service: PlayerService,
    imports: [HttpClientTestingModule],
    mocks: [HttpClient]
  });

  beforeEach(() => {
    spectator = createService();
    httpClient = spectator.inject(HttpClient);
    httpTestingController = spectator.inject(HttpTestingController);
  });

  describe('updatePlayerCache', async () => {
    const gameId = 'game Id';
    const playerId = 'player Id';
    const returnedPlayer: ApiPlayerInfo = {
      id: playerId,
      name: 'some player name'
    };
    httpTestingController
      .expectOne(`/api/game/${gameId}/player?id=${playerId}`)
      .flush([returnedPlayer]);
    const existingCache: PlayerCache = {
      'some player': EMPTY
    };

    spectator.service
      .updatePlayerCache(existingCache, gameId, playerId);
    const result = await existingCache[playerId].toPromise();

    expect(existingCache['some player']).toBeTruthy();
    expect(result).toBeTruthy();
    expect(result.id).toEqual(playerId);
  });
});
