import { Observable, Subject } from 'rxjs';
import { property } from 'src/pipes/property';
import { CommonStateModel, ICardId } from '../commonState/common-state-model';
import { CommonKnowledgeGame } from './game-client';

export class GameModel {
    private readonly gameSubject: Subject<CommonKnowledgeGame>;
    get DrawCount(): Observable<number> { return this.commonModel.DrawCount; }
    get Turn(): Observable<number> { return this.commonModel.Turn; }
    public readonly PlayerIds: Observable<readonly string[]>;
    get Discard(): Observable<readonly ICardId[]> { return this.commonModel.Discard; }
    get CurrentPlayerId(): Observable<string> { return this.commonModel.CurrentPlayerId; }
    get PlayersInRound(): Observable<readonly string[]> { return this.commonModel.PlayersInRound; }
    constructor(private readonly commonModel: CommonStateModel,
        gameState: CommonKnowledgeGame) {
        this.gameSubject = new Subject<CommonKnowledgeGame>();
        this.PlayerIds = this.gameSubject
            .pipe(
                property(g => g.players)
            );

        // this needs to happen after the subscriptions
        this.gameSubject.next(gameState);
    }
}
