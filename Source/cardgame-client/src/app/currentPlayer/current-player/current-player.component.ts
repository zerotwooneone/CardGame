import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CurrentPlayerModel } from '../current-player-model';
import { property } from 'src/pipes/property';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { CardModel } from 'src/app/card/card-model';
import { CardModelFactoryService } from 'src/app/card/card-model-factory.service';
import { IOtherPlayer } from 'src/app/gameBoard/game-board/game-board.component';
import { ChoiceOutput } from 'src/app/card/play-choice/play-choice.component';
import { CardDto } from 'src/app/game/game-client';

@Component({
  selector: 'cgc-current-player',
  templateUrl: './current-player.component.html',
  styleUrls: ['./current-player.component.scss']
})
export class CurrentPlayerComponent implements OnInit, OnChanges {
  @Input()
  player: CurrentPlayerModel;
  @Input()
  otherPlayers: readonly IOtherPlayer[];
  isTurn: Observable<boolean>;
  Name: Observable<string>;

  card1: CardModel | null;
  card2: CardModel | null;

  constructor(private cardModelFactory: CardModelFactoryService) { }

  ngOnInit(): void {  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.player) {
      if (!!this.player) {
        this.Name = this.player
          .Name
          .pipe(property(m => m));

        this.player.Card1.subscribe(c => this.card1 = this.mapToPlayableCard(c));
        this.player.Card2.subscribe(c => this.card2 = this.mapToPlayableCard(c));
        this.isTurn = this.player
          .IsTurn
          .pipe(
            property(p => p)
          );
      }
    }
  }

  mapToPlayableCard(card: CardDto | null): CardModel | null {
    if (!card) { return null; }
    const result = this.cardModelFactory.createPlayable(card);
    return result;
  }

  async play(card: CardModel | null, event: ChoiceOutput): Promise<any> {
    if (!this.player) { return; }
    if (card === null) { return; }
    // todo dialog for target and strength?
    const targetId = event.target;
    const guessValue = event.strength;
    const response = await this.player.play(card, targetId, guessValue);
  }
}


