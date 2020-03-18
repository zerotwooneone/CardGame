import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, shareReplay, first } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {

  constructor(private httpClient: HttpClient) { }
  getPlayersById(gameId: string, ...ids: string[]): Observable<IPlayerInfo[]> {
    const params = new HttpParams();
    ids.forEach(id => params.set('id', id));
    const result = this.httpClient
      .get<ApiPlayerInfo[]>(`/api/game/${gameId}/player`, {params})
      .pipe(
        map<ApiPlayerInfo[], IPlayerInfo[]>(a => a.map(p => ({ id: p.id, name: p.name }))),
        shareReplay()
      );
    return result;
  }

  updatePlayerCache(existing: PlayerCache, gameId: string, ...ids: string[]): void {
    const requestedKeys = Object.keys(existing);
    const maxQuerySize = 3;
    if (requestedKeys.length >= maxQuerySize
        || !ids
        || !ids.length
        || requestedKeys.length + ids.length >= (maxQuerySize * 2)) { return; }

    const params = new HttpParams();
    ids.forEach(id => params.set('id', id));
    const observable = this.getPlayersById(gameId, ...ids);
    const result = requestedKeys.reduce(
      (cache, i) => {
        cache[i] = observable.pipe(map(a => {
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
