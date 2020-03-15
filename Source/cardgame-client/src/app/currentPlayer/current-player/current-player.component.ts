import { Component, OnInit, Input } from '@angular/core';
import { ICurrentPlayer } from './ICurrentPlayer';

@Component({
  selector: 'cgc-current-player',
  templateUrl: './current-player.component.html',
  styleUrls: ['./current-player.component.scss']
})
export class CurrentPlayerComponent implements OnInit {

  constructor() { }
  @Input()
  player: ICurrentPlayer;
  ngOnInit(): void {
  }

}


