import { Injectable } from '@angular/core';
import { CardDto } from '../game/game-client';
import { CardModel } from './card-model';

@Injectable({
  providedIn: 'root'
})
export class CardModelFactoryService {
  constructor() { }

  createPlayable(card: CardDto): CardModel {
    return new CardModel(`${card.cardStrength}${card.variant}`, card.cardStrength);
  }
}
