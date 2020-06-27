import { EntityResult } from '../../core/entity/entity-result';
import { Round } from '../Round/round';
import { Card, UpdatedState } from './card';
import { CardStrength } from './CardStrength';
import { Player } from '../player/player';
import { Deck } from './deck';
import { CardId } from './card-id';

export class Princess extends Card {
    public Play(player?: Player,
                round?: Round,
                targetPlayer?: Player,
                deck?: Deck,
                targetCard?: Card,
                targetStrength?: CardStrength,
                drawCard?: () => CardId): EntityResult<UpdatedState> {
        const basePlayResult = super.Play(player, round, targetPlayer, deck, targetCard, targetStrength, drawCard);
        if (!basePlayResult.success) {
            return EntityResult.CreateError(basePlayResult.reason as string, basePlayResult.exception);
        }
        return this.Discard(player,
            round);
    }


    public Discard(player?: Player,
                   round?: Round): EntityResult<UpdatedState> {
        if (!round) { return EntityResult.CreateError('Round is required'); }
        const newRound = round.eliminate((player as Player).id);
        if (!newRound.success) {
            return EntityResult.CreateError(`error eliminating player: ${newRound.reason}`, newRound.exception);
        }
        return EntityResult.CreateSuccess({ newRound: newRound.value as Round });
    }
}


