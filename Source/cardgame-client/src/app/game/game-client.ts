import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { api } from 'src/pipes/api';
import { CardStrength } from '../domain/card/CardStrength';

export class GameClient {
    constructor(readonly gameId: string,
                private readonly httpClient: HttpClient) { }
    getCommonState(): Observable<CommonKnowledgeGame> {
        return this.httpClient
            .get<CommonKnowledgeGame>(`${this.getGameUrl()}`)
            .pipe(
                api(),
            );
    }
    getPlayer(playerId: string): Observable<PlayerDto> {
        return this.httpClient
            .get<PlayerDto>(`${this.getGameUrl()}/player/${playerId}`)
            .pipe(
                api(),
            );
    }
    play(request: PlayRequest): Observable<PlayResponse> {
        const response = this.httpClient.post<PlayResponse>(`${this.getGameUrl()}/play`, request)
            .pipe(
                api()
            );
        return response;
    }
    private getGameUrl(): string {
        return `/game/${this.gameId}`;
    }
}

export interface CommonKnowledgeGame {
    readonly id: string;
    readonly players: readonly CommonKnowledgePlayer[];
    readonly round: CommonKnowledgeRound;
    readonly winningPlayer?: string;
}

export interface CommonKnowledgePlayer {
    readonly id: string;
    readonly score: number;
}

export interface CommonKnowledgeRound {
    readonly id: number;
    readonly playerOrder: readonly string[];
    readonly turn: CommonKnowledgeTurn;
    readonly discard: readonly string[];
    readonly deckCount: number;
}

export interface CommonKnowledgeTurn {
    readonly id: number;
    readonly currentPlayer: string;
}

export interface PlayerDto {
    readonly hand: readonly CardDto[];
    readonly score?: number;
    readonly protected: boolean;
}

export interface CardDto {
    readonly cardStrength: CardStrength;
    readonly variant: number;
}

export interface PlayRequest {
    readonly playerId: string;
    readonly cardStrength: number;
    readonly cardVariant: number;
    readonly targetId?: string;
    readonly guessValue?: number;
}

export interface PlayResponse {
    correlationId?: string;
}
