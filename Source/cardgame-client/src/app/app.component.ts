import { Component, OnInit } from '@angular/core';
import { CommonStateFactoryService } from './commonState/common-state-factory.service';

@Component({
  selector: 'cgc-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'cardgame-client';

  constructor(private commonStateFactory: CommonStateFactoryService) {}

  async ngOnInit(): Promise<void> {
    this.commonStateFactory.create('some id')
      .then(commonState => {
        console.log(commonState);
      })
      .catch(err => {
        console.error(err);
      });
  }
}
