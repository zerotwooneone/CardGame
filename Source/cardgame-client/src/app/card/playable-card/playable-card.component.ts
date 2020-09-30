import { Component, OnInit, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { CardModel } from '../card-model';

@Component({
  selector: 'cgc-playable-card',
  templateUrl: './playable-card.component.html',
  styleUrls: ['./playable-card.component.scss']
})
export class PlayableCardComponent implements OnInit {

  @Input()
  card: CardModel;
  @Input()
  isTurn: boolean;

  get value(): number {
    return this.card?.value;
  }
  constructor() { }

  ngOnInit(): void {
  }

}
