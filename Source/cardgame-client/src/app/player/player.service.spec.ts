import { SpectatorHttp, createHttpFactory, HttpMethod } from '@ngneat/spectator';

import { PlayerService, PlayerCache, ApiPlayerInfo, IPlayerInfo } from './player.service';
import { EMPTY } from 'rxjs';

describe('PlayerService', () => {
  let spectator: SpectatorHttp<PlayerService>;
  const createService = createHttpFactory({
    service: PlayerService
  });

  beforeEach(() => {
    spectator = createService();
  });

  describe('updatePlayerCache', async () => {
    it('should set the players', async () => {
      const gameId = 'gameId';
      const playerId = 'playerId';
      const playerName = 'some player name';
      const returnedPlayer: ApiPlayerInfo = {
        id: playerId,
        name: playerName
      };

      const existingCache: PlayerCache = {
        'some player': EMPTY
      };

      spectator.service
        .updatePlayerCache(existingCache, gameId, playerId);
      const ob = existingCache[playerId];
      ob.subscribe();
      spectator
        .expectOne(`/api/game/${gameId}/player`, HttpMethod.GET)
        .flush([returnedPlayer]);
      const result = await ob.toPromise();

      expect(existingCache['some player']).toBeTruthy();
      expect(result).toEqual(jasmine.objectContaining({id: playerId, name: playerName} as IPlayerInfo));
    });
  });
  describe('getPlayersById', async () => {
    it('should work', async () => {
      spectator.service.getPlayersById('gameId', 'playerId').subscribe();

      const req = spectator.expectOne(`/api/game/gameId/player`, HttpMethod.GET);
      expect(true).toBeTruthy();
    });
  });
});
