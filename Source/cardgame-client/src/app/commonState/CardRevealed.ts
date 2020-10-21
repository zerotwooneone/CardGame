import { CardStrength } from '../domain/card/CardStrength';


export class CardRevealed {
    readonly correlationId: string;
    readonly gameId: string;
    readonly playerId: string;
    readonly targetCardStrength: CardStrength;
    readonly targetCardVariant: number;
    readonly targetId: string;
}


