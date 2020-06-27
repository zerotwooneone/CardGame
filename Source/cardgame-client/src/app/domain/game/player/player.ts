import { PlayerId } from './player-id';
import { Score } from './score';
import { EntityResult } from '../../core/entity/entity-result';
import { Name } from './name';
import { Hand } from '../card/hand';

export class Player {
    protected constructor(readonly id: PlayerId,
                          protected name: Name,
                          protected score: Score,
                          protected hand: Hand) { }
    public get Name(): Name { return this.name; }
    public get Score(): Score { return this.score; }
    public get Hand(): Hand { return this.hand; }
    protected protected: boolean;
    public get Protected(): boolean { return this.protected; }
    public static Factory(id: PlayerId,
                          name: Name,
                          hand: Hand,
                          score: Score = Score.Default): EntityResult<Player> {
        if (!id) {
            return EntityResult.CreateError('id is required');
        }
        if (!name) {
            return EntityResult.CreateError('name is required');
        }
        if (!score) {
            return EntityResult.CreateError('score is required');
        }
        if (!hand) {
            return EntityResult.CreateError('hand is required');
        }
        return EntityResult.CreateSuccess(new Player(id, name, score, hand));
    }
    public IncrementScore(): void { this.score = this.score.GetIncremented(); }
    public ChangeName(value: Name): void { this.name = value; }
    public Exchange(hand: Hand): Hand {
        const result = this.hand;
        this.SetHand(hand);
        return result;
    }
    public SetHand(hand: Hand): void {
        this.hand = hand;
    }
    Protect() {
        this.protected = true;
        //todo: need to implement unprotect at end of turn
    }
}
