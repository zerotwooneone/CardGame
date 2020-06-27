import { EntityResult } from '../../core/entity/entity-result';
import { Round } from '../Round/round';
import { Card, UpdatedState } from './card';
import { Hand } from './hand';
import { CardStrength } from './CardStrength';
import { Player } from '../player/player';
import { Deck } from './deck';
import { CardId } from './card-id';

export class Prince extends Card {
    public Play(player?: Player,
                round?: Round,
                targetPlayer?: Player,
                deck?: Deck,
                targetCard?: Card,
                targetStrength?: CardStrength,
                drawCard?: () => CardId): EntityResult<UpdatedState> {
        if (!player) { return EntityResult.CreateError('Player is required'); }
        if (!targetPlayer) { return EntityResult.CreateError('target player is required'); }
        if (!deck) { return EntityResult.CreateError('deck is required'); }
        if (!round) { return EntityResult.CreateError('round is required'); }
        if (!targetCard) { return EntityResult.CreateError('targetCard is required'); }
        if (!drawCard) { return EntityResult.CreateError('drawCard is required'); }
        const basePlayResult = super.Play(player, round, targetPlayer, deck, targetCard, targetStrength, drawCard);
        if (!basePlayResult.success) {
            return EntityResult.CreateError(basePlayResult.reason as string, basePlayResult.exception);
        }

        const discardResult = targetCard?.Discard(targetPlayer, round);
        if (!discardResult.success) {
            return EntityResult.CreateError(
                `could not discard card: ${discardResult.reason}`, discardResult.exception);
        }
        const discardedDeck = deck.Discard(targetCard.id);
        if (!discardedDeck.success) {
            return EntityResult.CreateError(
                `could not discard from the deck: ${discardedDeck.reason}`, discardedDeck.exception);
        }
        const newDeck = (discardedDeck.value as Deck).Draw();
        if (!newDeck.success) {
            return EntityResult.CreateError(
                `could not draw from the deck: ${newDeck.reason}`, newDeck.exception);
        }

        const hand = Hand.Factory(drawCard());
        if (!hand.success) {
            EntityResult.CreateError(`could not create a new hand: ${hand.reason}`, hand.exception);
        }
        targetPlayer.SetHand(hand.value as Hand);

        return EntityResult.CreateSuccess({ newDeck: newDeck.value as Deck });
    }
}
