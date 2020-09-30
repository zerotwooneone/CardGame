import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
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
  @Output()
  playClick = new EventEmitter<any>();

  get value(): number {
    return this.card?.value;
  }
  constructor() { }

  ngOnInit(): void {
  }

  onPlayClick(): void {
    this.playClick.emit(true);
  }

}
