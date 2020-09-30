import { Injectable } from '@angular/core';
import { CardModel } from './card-model';

@Injectable({
  providedIn: 'root'
})
export class CardModelFactoryService {
  constructor() { }

  createPlayable(cardId: string): CardModel {
    return new CardModel(cardId, parseInt(cardId.substr(0, 1), 10));
  }
}
