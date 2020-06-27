import { CardId } from './card-id';
import { ValueResult } from '../../core/value/value-result';
import { CardStrength } from './CardStrength';
import { EntityResult } from '../../core/entity/entity-result';
import { Deck } from './deck';
import { Round } from '../Round/round';
import { Hand } from './hand';
import { Princess } from './Princess';
import { Player } from '../player/player';

export abstract class Card {
    public static Factory(id: CardId): ValueResult<Card> {
        if (!id) {
            return ValueResult.CreateError<Card>('id is required');
        }
        switch (id.value.value) {
            case CardStrength.Princess: {
                return ValueResult.CreateSuccess<Card>(new Princess(id));
                break;
            }
            default: {
                return ValueResult.CreateError(`unknown card type ${id.value.value}`);
            }
        }
    }
    protected constructor(readonly id: CardId) {}

    public GetMustPlay(other: CardId): boolean {
        return false;
    }

    public Play(player?: Player,
                round?: Round,
                targetPlayer?: Player,
                deck?: Deck,
                targetCard?: Card,
                targetStrength?: CardStrength,
                drawCard?: () => CardId): EntityResult<UpdatedState> {
        return EntityResult.CreateSuccess({});
    }

    public Discard(player?: Player,
                   round?: Round): EntityResult<UpdatedState> {
        return EntityResult.CreateSuccess({});
    }
}

export interface UpdatedState {
    newRound?: Round;
    newDeck?: Deck;
    //newDeck?: Deck note-- deck and turn changes are the responsability of the game engine, not the card
}
