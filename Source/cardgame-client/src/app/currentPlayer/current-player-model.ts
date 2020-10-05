import { Observable, of, Subject } from 'rxjs';
import { property } from 'src/pipes/property';
import { CardModel } from '../card/card-model';
import { CardStrength } from '../domain/card/CardStrength';
import { CardDto, GameClient, PlayerDto } from '../game/game-client';
import { GameModel } from '../game/game-model';

export class CurrentPlayerModel {
    readonly Id: string;
    readonly Name: Observable<string>;
    readonly Card1: Observable<CardDto | null>;
    readonly Card2: Observable<CardDto | null>;
    readonly IsTurn: Observable<boolean>;
    private readonly playerSubject: Subject<PlayerDto>;
    constructor(private readonly id: string,
                playerPromise: Promise<PlayerDto>,
                private gameClient: GameClient,
        private gameModel: GameModel) {
        this.playerSubject = new Subject<PlayerDto>();

        // todo: get the player name from the dto
        this.Name = of(id);
        this.Id = id;

        this.Card1 = this.playerSubject
            .pipe(
                property(p => p.hand[0])
            );
        this.Card2 = this.playerSubject
            .pipe(
                property(p => p.hand.length > 1 ? p.hand[1] : null)
            );
        this.IsTurn = this.gameModel
            .CurrentPlayerId
            .pipe(
                property(cp => cp === id)
            );
        playerPromise.then(p => {
            this.playerSubject.next(p);
        });
    }
    async refresh(): Promise<any> {
        const apiObservable = this.gameClient.getPlayer(this.id);
        apiObservable.subscribe(p => this.playerSubject.next(p));
        return apiObservable.toPromise();
    }
    async play(card: CardModel,
        targetId: string | null,
        guessValue: CardStrength | null): Promise<any> {
        // todo: move this call?
        const response = await this.gameClient.play({
            cardStrength: card.cardStrength,
            cardVariant: parseInt(card.id.substr(1, 1), 10),
            playerId: this.id,
            guessValue: guessValue ?? undefined,
            targetId: targetId ?? undefined
        });
    }
}
