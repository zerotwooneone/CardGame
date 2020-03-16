import { Injectable } from '@angular/core';
import { CardModel } from './card-model';

@Injectable({
  providedIn: 'root'
})
export class CardModelFactoryService {
  private readonly cards: {[id: string]: CardModel} = {
    'card 1 id': new CardModel('card 1 id', 9),
    'card 2 id': new CardModel('card 2 id', 10)
  };
  constructor() { }

  createPlayable(cardId: string): CardModel {
    return this.cards[cardId];
  }
}
