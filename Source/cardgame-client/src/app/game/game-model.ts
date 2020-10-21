import { merge, Observable, Subject } from 'rxjs';
import { concatAll, distinctUntilChanged, map, shareReplay, switchMap } from 'rxjs/operators';
import { property } from 'src/pipes/property';
import { CardRevealed } from '../commonState/CardRevealed';
import { CommonStateModel, ICardId } from '../commonState/common-state-model';
import { CommonKnowledgeGame, CommonKnowledgePlayer } from './game-client';

export class GameModel {
    private readonly gameSubject: Subject<CommonKnowledgeGame>;
    readonly PlayerScores: Observable<CommonKnowledgePlayer>;
    get DrawCount(): Observable<number> { return this.commonModel.DrawCount; }
    get Turn(): Observable<number> { return this.commonModel.Turn; }
    public readonly PlayerIds: Observable<readonly string[]>;
    get Discard(): Observable<readonly ICardId[]> { return this.commonModel.Discard; }
    get CurrentPlayerId(): Observable<string> { return this.commonModel.CurrentPlayerId; }
    get PlayersInRound(): Observable<readonly string[]> { return this.commonModel.PlayersInRound; }
    constructor(private readonly commonModel: CommonStateModel,
                gameState: CommonKnowledgeGame,
        readonly CardRevealedObservable: Observable<CardRevealed>) {
        this.gameSubject = new Subject<CommonKnowledgeGame>();
        this.PlayerIds = this.gameSubject
            .pipe(
                distinctUntilChanged((old, newer) => old.players.length === newer.players.length),
                property(g => g.players.map(p => p.id))
            );
        const players = this.gameSubject
            .pipe(
                map(x => x.players.slice()),
            );
        const maxPlayers = 4;
        this.PlayerScores = merge(
            players.pipe(
                concatAll(),
            ),
            players.pipe(
                switchMap(p => commonModel.Player1Score.pipe(map(s => this.getPlayer(p[0], s))))
            ),
            players.pipe(
                switchMap(p => commonModel.Player2Score.pipe(map(s => this.getPlayer(p[1], s))))
            ),
            players.pipe(
                switchMap(p => commonModel.Player3Score.pipe(
                    map(s => this.getPlayer(p[2], s)))
                )
            ),
            players.pipe(
                switchMap(p => commonModel.Player4Score.pipe(
                    map(s => this.getPlayer(p[3], s)))
                )
            )
        ).pipe(
            shareReplay(maxPlayers)
        );
        this.PlayerScores.subscribe(); // todo: refactor play scores

        // this needs to happen after the subscriptions
        this.gameSubject.next(gameState);
    }

    private getPlayer(player: CommonKnowledgePlayer, score: number) {
        const commonPlayer: CommonKnowledgePlayer = {
            id: player.id,
            score
        };
        return commonPlayer;
    }
}