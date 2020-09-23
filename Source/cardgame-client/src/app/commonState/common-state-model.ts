import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { CommonGameStateChanged } from './CommonGameStateChanged';

export class CommonStateModel {
    private readonly stateChangedObservable: Observable<CommonGameStateChanged>;
    public readonly DrawCount: Observable<number>;
    public readonly PlayerIds: Observable<string[]>;
    public readonly Discard: Observable<ICardId[]>;
    public readonly CurrentPlayerId: Observable<string>;
    public readonly PlayersInRound: Observable<string[]>;
    constructor(private stateObservable: Observable<CommonGameStateChanged>) {
        this.stateChangedObservable = stateObservable;

        this.DrawCount = this
            .stateChangedObservable
            .pipe(
                property(m => m.drawCount)
            );

        // todo fix player ids
        this.PlayerIds = this
            .stateChangedObservable
            .pipe(
                property(m => ['9b644228-6c7e-4caa-becf-89e093ee299f', '5e96fafb-83b2-4e72-8afa-0e6a8f12345f'])
            );
        this.PlayersInRound = this
            .PlayerIds;

        this.Discard = this
            .stateChangedObservable
            .pipe(
                map(m => this.GetCards(m.discard)),
                property(m => m)
            );
        this.CurrentPlayerId = this
            .stateChangedObservable
            .pipe(
                property(m => m.currentPlayer)
        );
     }
    GetCards(Discard: string[]): ICardId[] {
        return Discard.map(c => ({
            strength: parseInt(c.slice(0, 1), 10),
            varient: parseInt(c.slice(1, 1), 10)
        }));
    }
}

export interface ICardId {
    readonly strength: number;
    readonly varient: number;
}
