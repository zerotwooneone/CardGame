import { PlayerId } from './player-id';
import { Score } from './score';
import { EntityResult } from '../../core/entity/entity-result';
import { Name } from './name';

export class Player {
    protected constructor(readonly id: PlayerId,
                          protected name: Name,
                          protected score: Score) { }
    public get Name(): Name { return this.name; }
    public get Score(): Score { return this.score; }
    public static Factory(id: PlayerId,
                          name: Name,
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
        return EntityResult.CreateSuccess(new Player(id, name, score));
    }
    public IncrementScore(): void { this.score = this.score.GetIncremented(); }
    public ChangeName(value: Name): void { this.name = value; }
}
