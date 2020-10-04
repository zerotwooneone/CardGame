
export class CommonGameStateChanged {
    readonly playerOrder: readonly string[];
    readonly player1Score: number;
    readonly player2Score: number;
    readonly player3Score: number;
    readonly player4Score: number;
    readonly round: number;
    readonly turn: number;
    readonly discard: string[];
    readonly winningPlayer: string;
    readonly correlationId: string;
    readonly gameId: string;
    readonly drawCount: number;
}
