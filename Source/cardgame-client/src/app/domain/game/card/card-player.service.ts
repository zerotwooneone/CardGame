import { Injectable } from '@angular/core';
import { CardId } from './card-id';
import { PlayerId } from '../player/player-id';

@Injectable({
  providedIn: 'root'
})
export class CardPlayerService {

  constructor(protected roundRepositoryService: RoundRepositoryService) { }

  public play(card: CardId, player: PlayerId): Promise<any> {
    
  }
}
