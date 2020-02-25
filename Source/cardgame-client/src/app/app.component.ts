import { Component, OnInit } from '@angular/core';
import { CommonStateFactoryService } from './commonState/common-state-factory.service';
import { ClientFactoryService } from './client/client-factory.service';
import { take, first, timeout } from 'rxjs/operators';

@Component({
  selector: 'cgc-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'cardgame-client';

  constructor(private commonStateFactory: CommonStateFactoryService,
              private clientFactory: ClientFactoryService) {}

  async ngOnInit(): Promise<void> {
    try {
      const commonState = await this.commonStateFactory.create('some id');
    } catch (error) {
      console.error(error);
    }

    try {
      const c = await this.clientFactory.Create({Id: 'something'});
      console.log('created client');
      const state = await c.State.pipe(first(), timeout(100)).toPromise();
      console.log(state);
    } catch (error) {
      console.error(error);
    }

  }
}
