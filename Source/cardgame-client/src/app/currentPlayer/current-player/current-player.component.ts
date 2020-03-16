import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { IPlayableCard } from './IPlayableCard';
import { CurrentPlayerModel } from '../current-player-model';
import { property } from 'src/pipes/property';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'cgc-current-player',
  templateUrl: './current-player.component.html',
  styleUrls: ['./current-player.component.scss']
})
export class CurrentPlayerComponent implements OnInit, OnChanges {
  @Input()
  player: CurrentPlayerModel;
  Name: Observable<string>;

  card1: Observable<IPlayableCard>;
  card2: Observable<IPlayableCard>;

  constructor() { }

  ngOnInit(): void {  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.player) {
      console.warn('player changed');
      this.Name = this.player
        .Name
        .pipe(property(m => m));

      this.card1 = this.player
        .Card1
        .pipe(
          map(this.mapToPlayableCard),
          property(m => m)
        );
      this.card2 = this.player
        .Card2
        .pipe(
          map(this.mapToPlayableCard),
          property(m => m)
        );
    }
  }

  mapToPlayableCard(cardId: string): IPlayableCard {
    // todo: need method (service?) to provide details about a card given an id
    return { Id: cardId, Value: 9 };
  }
}


