import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { CommonGameStateChanged } from './CommonGameStateChanged';

export class CommonStateModel {
    private readonly stateChangedObservable: Observable<CommonGameStateChanged>;
    public readonly DrawCount: Observable<number>;
    public readonly Turn: Observable<number>;
    public readonly PlayerIds: Observable<readonly string[]>;
    public readonly Discard: Observable<readonly ICardId[]>;
    public readonly CurrentPlayerId: Observable<string>;
    public readonly PlayersInRound: Observable<readonly string[]>;
    constructor(private stateObservable: Observable<CommonGameStateChanged>) {
        this.stateChangedObservable = stateObservable;

        this.DrawCount = this
            .stateChangedObservable
            .pipe(
                property(m => m.drawCount)
            );

        this.PlayersInRound = this
            .stateChangedObservable
            .pipe(
                property(m => m.playerOrder)
            );

        this.Discard = this
            .stateChangedObservable
            .pipe(
                property(m => this.GetCards(m.discard))
            );
        this.CurrentPlayerId = this
            .stateChangedObservable
            .pipe(
                property(m => m.playerOrder[0])
        );
        this.Turn = this
            .stateChangedObservable
            .pipe(
                property(s => s.turn)
            );
     }
    GetCards(discard: string[]): ICardId[] {
        const converted = discard.map(c => ({
            strength: parseInt(c.slice(0, 1), 10),
            varient: parseInt(c.slice(1, 1), 10)
        }));
        return converted;
    }
}

export interface ICardId {
    readonly strength: number;
    readonly varient: number;
}
