import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'cgc-other-player',
  templateUrl: './other-player.component.html',
  styleUrls: ['./other-player.component.scss']
})
export class OtherPlayerComponent implements OnInit {

  @Input() name: string;
  @Input() isInRound: boolean;
  constructor() { }

  ngOnInit(): void {
  }

}


