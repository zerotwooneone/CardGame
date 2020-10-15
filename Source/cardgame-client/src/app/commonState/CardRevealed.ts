import { CardStrength } from '../domain/card/CardStrength';


export class CardRevealed {
    readonly CorrelationId: string;
    readonly GameId: string;
    readonly PlayerId: string;
    readonly TargetCardStrength: CardStrength;
    readonly TargetCardVariant: number;
    readonly TargetId: string;
}


