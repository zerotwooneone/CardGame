
export class CommonGameStateChanged {
    readonly currentPlayer: string;
    readonly player1InRound: boolean;
    readonly Player2InRound: boolean;
    readonly player3InRound: boolean;
    readonly player4InRound: boolean;
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
