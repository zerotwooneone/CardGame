import { EntityResult } from '../../core/entity/entity-result';
import { Round } from '../Round/round';
import { Card, UpdatedState } from './card';
import { CardStrength } from './CardStrength';
import { Player } from '../player/player';
import { Deck } from './deck';
import { CardId } from './card-id';

export class King extends Card {
    public Play(player?: Player,
                round?: Round,
                targetPlayer?: Player,
                deck?: Deck,
                targetCard?: Card,
                targetStrength?: CardStrength,
                drawCard?: () => CardId): EntityResult<UpdatedState> {
        if (!player) { return EntityResult.CreateError('Player is required'); }
        if (!targetPlayer) { return EntityResult.CreateError('target player is required'); }
        if (!round) { return EntityResult.CreateError('round is required'); }
        if (!round.remains(targetPlayer.id)) { return EntityResult.CreateError('target must be in round'); }
        const basePlayResult = super.Play(player, round, targetPlayer, deck, targetCard, targetStrength, drawCard);
        if (!basePlayResult.success) {
            return EntityResult.CreateError(basePlayResult.reason as string, basePlayResult.exception);
        }
        const hand = targetPlayer.Exchange(player.Hand);
        player.SetHand(hand);
        return this.Discard(player,
            round);
    }
}
