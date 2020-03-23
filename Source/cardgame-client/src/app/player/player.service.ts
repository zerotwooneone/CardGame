import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, shareReplay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {

  constructor(private httpClient: HttpClient) { }
  getPlayersById(gameId: string, ...ids: string[]): Observable<IPlayerInfo[]> {
    const params = ids.reduce((p, id) => p.append('id', id),
      new HttpParams());
    const result = this.httpClient
      .get<ApiPlayerInfo[]>(`/api/game/${gameId}/player`, { params })
      .pipe(
        map<ApiPlayerInfo[], IPlayerInfo[]>(a => a.map(p => ({ id: p.id, name: p.name }))),
        shareReplay()
      );
    // return result;
    result.subscribe();
    return of([
        { id: '1', name: 'Player 1'},
        { id: '2', name: 'Player 2' },
        { id: '3', name: 'Player 3' } ] as IPlayerInfo[]);
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

export interface ApiPlayerInfo {
  readonly id: string;
  readonly name: string;
}

export interface PlayerCache {
  [id: string]: Observable<IPlayerInfo>;
}

export interface IPlayerInfo {
  readonly id: string;
  readonly name: string;
}
