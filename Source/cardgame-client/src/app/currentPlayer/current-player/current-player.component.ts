import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CurrentPlayerModel } from '../current-player-model';
import { property } from 'src/pipes/property';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { CardModel } from 'src/app/card/card-model';
import { CardModelFactoryService } from 'src/app/card/card-model-factory.service';

@Component({
  selector: 'cgc-current-player',
  templateUrl: './current-player.component.html',
  styleUrls: ['./current-player.component.scss']
})
export class CurrentPlayerComponent implements OnInit, OnChanges {
  @Input()
  player: CurrentPlayerModel;
  isTurn: Observable<boolean>;
  Name: Observable<string>;

  card1: Observable<CardModel>;
  card2: Observable<CardModel>;

  constructor(private cardModelFactory: CardModelFactoryService) { }

  ngOnInit(): void {  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.player) {
      if (!!this.player) {
        this.Name = this.player
          .Name
          .pipe(property(m => m));

        this.card1 = this.player
          .Card1
          .pipe(
            property(m => this.mapToPlayableCard(m)),
        );
        this.card2 = this.player
          .Card2
          .pipe(
            property(m => this.mapToPlayableCard(m))
          );
        this.isTurn = this.player
          .IsTurn
          .pipe(
            property(p => p)
          );
      }
    }
  }

  mapToPlayableCard(cardId: string): CardModel {
    const result = this.cardModelFactory.createPlayable(cardId);
    return result;
  }
}


