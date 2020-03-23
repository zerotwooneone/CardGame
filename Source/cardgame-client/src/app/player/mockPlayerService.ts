import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { IPlayerService, PlayerCache, IPlayerInfo, PlayerService } from './player.service';
import { Injectable } from '@angular/core';

@Injectable()
export class MockPlayerService implements IPlayerService {

    getPlayersById(gameId: string, ...ids: string[]): Observable<IPlayerInfo[]> {
        return of([
            { id: '1', name: 'Player 1' },
            { id: '2', name: 'Player 2' },
            { id: '3', name: 'Player 3' }] as IPlayerInfo[]);
    }

    updatePlayerCache(existing: PlayerCache, gameId: string, ...ids: string[]): void {
        const maxQuerySize = 3;
        if (!ids
            || !ids.length
            || ids.length > maxQuerySize) { return; }

        const observable = this.getPlayersById(gameId, ...ids);
        const result = ids.reduce(
            (cache, i) => {
                cache[i] = observable.pipe(
                    map(a => {
                        const r = a.find(p => p.id === i);
                        if (r === undefined) {
                            throw Error('could not find expected player id');
                        }
                        return r as IPlayerInfo;
                    }));
                return cache;
            }
            , existing);
    }
}