import { CardStrength } from '../domain/card/CardStrength';

export class CardModel {
    constructor(public readonly id: string,
        public readonly value: CardStrength) { }
}
