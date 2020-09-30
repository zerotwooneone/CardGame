import { Observable, of, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { CommonStateModel } from '../commonState/common-state-model';
import { CardDto, GameClient, PlayerDto } from '../game/game-client';

export class CurrentPlayerModel {
    readonly Name: Observable<string>;
    readonly Card1: Observable<string>;
    readonly Card2: Observable<string>;
    readonly IsTurn: Observable<boolean>;
    private readonly playerSubject: Subject<PlayerDto>;
    constructor(private readonly id: string,
        playerPromise: Promise<PlayerDto>,
        private gameClient: GameClient,
        private commonState: CommonStateModel) {
        this.playerSubject = new Subject<PlayerDto>();

        // todo: get the player name from the dto
        this.Name = of(id);

        this.Card1 = this.playerSubject
            .pipe(
                property(p => this.convertToCard(p.hand, 0))
            );
        this.Card2 = this.playerSubject
            .pipe(
                property(p => this.convertToCard(p.hand, 1))
            );
        this.IsTurn = this.commonState
            .CurrentPlayerId
            .pipe(
                property(cp => cp === id)
            );
        playerPromise.then(p => {
            this.playerSubject.next(p);
        });
    }
    convertToCard(cards: readonly CardDto[], index: number): string {
        if (!cards || index >= cards.length || index < 0) { return ''; }
        const result = `${cards[index]?.cardStrength}${cards[index]?.variant}`;
        return result;
    }
}
