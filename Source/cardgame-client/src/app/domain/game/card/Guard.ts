import { Card, PlayContext } from './card';
import { CardStrength } from './CardStrength';
import { PlayerId } from '../player/player-id';

export class Guard extends Card {
    protected async PlayImplementation(playContext: PlayContext): Promise<any> {
        const guess = await playContext.getOtherPlayerAndCardGuess(playContext.player);

        if (!guess.aborted) {
            if (guess.value?.strength === CardStrength.Guard) {
                // if the player guesses a guard, then they waste their turn
                return;
            }
            const targetCard = await playContext.getHand(guess.value?.target as PlayerId);
            if (guess.value?.strength === targetCard.value.value) {
                playContext.eliminate(guess.value.target);
            }
        }
    }
}
