import { ValueResult } from '../../core/value/value-result';
import { PlayerId } from '../player/player-id';

export class Round {
    public static Factory(remaining: readonly PlayerId[],
                          eliminated: readonly PlayerId[]): ValueResult<Round> {
        if (!remaining ||
            remaining.length <= 0) {
            return ValueResult.CreateError<Round>('remaining cannot be empty');
        }
        return ValueResult.CreateSuccess<Round>(new Round(remaining.slice(), eliminated.slice()));
    }
    protected constructor(readonly remaining: readonly PlayerId[],
                          readonly eliminated: readonly PlayerId[]) { }

    public Equals(other: Round): boolean {
        if (this === other) { return true; }
        if (!other ||
            other.remaining.length !== this.remaining.length ||
            other.eliminated.length !== this.eliminated.length) {
            return false;
        }
        for (const id of other.eliminated) {
            if (this.eliminated.some(e => e.Equals(id))) {
                return false;
            }
        }
        for (const id of other.remaining) {
            if (this.remaining.some(e => e.Equals(id))) {
                return false;
            }
        }
        return true;
    }

    public eliminate(player: PlayerId): ValueResult<Round> {
        if (!this.remains(player)) {
            return ValueResult.CreateError(`remaining does not contain ${player}`);
        }
        if (this.eliminated.some(e => e.Equals(player))) {
            return ValueResult.CreateError(`already eliminated ${player}`);
        }
        return Round.Factory(this.remaining.filter(e => !e.Equals(player)), this.eliminated.concat(player));
    }
    remains(player: PlayerId):boolean {
        return this.remaining.some(e => e.Equals(player));
    }
}
