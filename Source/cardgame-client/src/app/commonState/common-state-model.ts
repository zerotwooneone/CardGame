import { IOpenConnection } from '../hub/IOpenConnection';
import { Observable } from 'rxjs';
import { property } from 'src/pipes/property';

export class CommonStateModel {
    private readonly stateChangedObservable: Observable<ICommonStateChanged>;
    public readonly StateId: Observable<string>;
    public readonly DrawCount: Observable<number>;
    public readonly PlayerIds: Observable<string[]>;
    public readonly Discard: Observable<ICard[]>;
    public readonly CurrentPlayerId: Observable<string>;
    public readonly PlayersInRound: Observable<string[]>;
    constructor(private connection: IOpenConnection) {
        this.stateChangedObservable = connection.register<ICommonStateChanged>('changed');

        this.StateId = this
            .stateChangedObservable
            .pipe(
                property(m => m.StateId)
            );

        this.DrawCount = this
            .stateChangedObservable
            .pipe(
                property(m => m.DrawCount)
            );

        this.PlayerIds = this
            .stateChangedObservable
            .pipe(
                property(m => m.PlayerIds)
            );
        this.Discard = this
            .stateChangedObservable
            .pipe(
                property(m => m.Discard)
            );
        this.CurrentPlayerId = this
            .stateChangedObservable
            .pipe(
                property(m => m.CurrentPlayerId)
            );
        this.PlayersInRound = this
            .stateChangedObservable
            .pipe(
                property(m => m.PlayersInRound)
            );
     }

    onCommonStateChanged(onCommonStateChanged: ICommonStateChanged) {
        console.log(`change received ${onCommonStateChanged.StateId}`);
        console.log(onCommonStateChanged);
    }
}

export interface ICommonStateChanged {
    readonly StateId: string;
    readonly DrawCount: number;
    readonly PlayerIds: string[];
    readonly Discard: ICard[];
    readonly CurrentPlayerId: string;
    readonly PlayersInRound: string[];
    [key: string]: any;
}

export interface ICard {
    readonly Id: string;
    readonly Value: number;
}
