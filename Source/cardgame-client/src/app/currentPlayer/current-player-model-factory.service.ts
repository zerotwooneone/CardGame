import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { CurrentPlayerModel } from './current-player-model';
import { property } from 'src/pipes/property';

@Injectable({
  providedIn: 'root'
})
export class CurrentPlayerModelFactoryService {
  constructor() { }

  getById(currentPlayerId: string): Observable<CurrentPlayerModel> {
    return of(new CurrentPlayerModel(currentPlayerId))
      .pipe(property(m => m));
  }
}
