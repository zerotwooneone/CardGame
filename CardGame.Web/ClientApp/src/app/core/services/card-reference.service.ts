import { Injectable } from '@angular/core';
import {CardReferenceItem} from '../models/cardReferenceItem';

@Injectable({
  providedIn: 'root'
})
export class CardReferenceService {
  private readonly cardReferenceData: CardReferenceItem[] = [
    {
      rank: 8,
      name: 'Princess',
      countInDeck: 1,
      effectDescription: 'If you discard this card for any reason, you are out of the round.',
      specialNote: 'WARNING: Discarding the Princess eliminates you!'
    },
    {
      rank: 7,
      name: 'Countess',
      countInDeck: 1,
      effectDescription: 'If you have this card and the King or Prince in your hand, you must discard the Countess.',
      specialNote: 'You are not forced to play her if you draw her and your other card is not a King or Prince.'
    },
    {
      rank: 6,
      name: 'King',
      countInDeck: 1,
      effectDescription: 'Trade the card in your hand with the card held by another player of your choice.'
    },
    {
      rank: 5,
      name: 'Prince',
      countInDeck: 2,
      effectDescription: 'Choose any player (including yourself). That player discards the card in their hand and draws a new card. If the deck is empty, they draw the set-aside card.'
    },
    {
      rank: 4,
      name: 'Handmaid',
      countInDeck: 2,
      effectDescription: 'Until your next turn, ignore all effects from other players\' cards.'
    },
    {
      rank: 3,
      name: 'Baron',
      countInDeck: 2,
      effectDescription: 'Choose another player. You and that player secretly compare your hands. The player with the lower rank card is out of the round. If it\'s a tie, nothing happens.'
    },
    {
      rank: 2,
      name: 'Priest',
      countInDeck: 2,
      effectDescription: 'Look at another player\'s hand (do not reveal it to others).'
    },
    {
      rank: 1,
      name: 'Guard',
      countInDeck: 5,
      effectDescription: 'Choose another player and name a non-Guard card. If that player has that card, they are out of the round.',
      specialNote: 'You cannot guess "Guard".'
    }
    // Note: Card counts are for the standard 16-card deck.
  ];

  constructor() { }

  /**
   * Gets all card reference items, sorted by rank in descending order.
   */
  getAllCardReferences(): CardReferenceItem[] {
    // Return a copy to prevent modification of the original array
    return [...this.cardReferenceData].sort((a, b) => b.rank - a.rank);
  }

  /**
   * Gets reference information for a specific card by its rank (numeric value).
   */
  getCardReferenceByRank(rank: number): CardReferenceItem | undefined {
    return this.cardReferenceData.find(card => card.rank === rank);
  }
}
