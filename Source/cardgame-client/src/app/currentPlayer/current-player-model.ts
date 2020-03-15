import { Observable, of } from 'rxjs';

export class CurrentPlayerModel {
    readonly Name: Observable<string> = of('some player name');
    readonly Card1: Observable<string> = of('card 1 id');
    readonly Card2: Observable<string> = of('card 2 id');
    constructor(private readonly id: string) { }
}
