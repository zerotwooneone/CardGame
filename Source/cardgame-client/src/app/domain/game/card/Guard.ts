import { EntityResult } from '../../core/entity/entity-result';
import { Round } from '../Round/round';
import { Card, UpdatedState } from './card';
import { CardStrength } from './CardStrength';
import { Player } from '../player/player';
import { Deck } from './deck';
import { CardId } from './card-id';

export class Guard extends Card {
    public Play(player?: Player,
                round?: Round,
                targetPlayer?: Player,
                deck?: Deck,
                targetCard?: Card,
                targetStrength?: CardStrength,
                drawCard?: () => CardId): EntityResult<UpdatedState> {

        if (!round) { return EntityResult.CreateError('round is required'); }
        if (!player) { return EntityResult.CreateError('player is required'); }
        if (!targetPlayer) { return EntityResult.CreateError('target player is required'); }
        if (!targetStrength) { return EntityResult.CreateError('target card type is required'); }
        if (targetPlayer.id.Equals(player.id)) { return EntityResult.CreateError('cannot target self'); }
        if (targetStrength === CardStrength.Guard) { return EntityResult.CreateError('cannot target Guard'); }
        const basePlayResult = super.Play(player, round, targetPlayer, deck, targetCard, targetStrength, drawCard);
        if (!basePlayResult.success) {
            return EntityResult.CreateError(basePlayResult.reason as string, basePlayResult.exception);
        }

        if (targetPlayer.Hand.Contains(targetStrength)) {
            const newRound = round.eliminate(targetPlayer.id);
            if (!newRound.success) {
                return EntityResult.CreateError(`error eliminating ${newRound.reason}`, newRound.exception);
            }
            return EntityResult.CreateSuccess({ newRound: newRound.value as Round });
        }
        return EntityResult.CreateSuccess({});
    }
}
