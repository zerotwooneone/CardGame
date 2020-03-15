import { Component, OnInit, Input } from '@angular/core';
import { IPlayer } from './IPlayer';

@Component({
  selector: 'cgc-other-player',
  templateUrl: './other-player.component.html',
  styleUrls: ['./other-player.component.scss']
})
export class OtherPlayerComponent implements OnInit {

  @Input() player: IPlayer;
  constructor() { }

  ngOnInit(): void {
  }

}


