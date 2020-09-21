import { Component, OnInit } from '@angular/core';
import { CommonStateFactoryService } from './commonState/common-state-factory.service';
import { ClientFactoryService } from './client/client-factory.service';
import { take, first, timeout } from 'rxjs/operators';
import { Guid } from './domain/core/id/guid';

@Component({
  selector: 'cgc-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'cardgame-client';
  gameId = 'app conponent game id';

  constructor(private commonStateFactory: CommonStateFactoryService,
              private clientFactory: ClientFactoryService) {}

  async ngOnInit(): Promise<void> {
    try {
      const commonState = await this.commonStateFactory.create('some id');
    } catch (error) {
      console.error(error);
    }

    try {
      const c = await this.clientFactory.Create({ GameId: '96a8f4b0-7800-4c26-80b6-fd66f286140f' });
      console.log('created client');
      const state = await c.State.pipe(first(), timeout(100)).toPromise();
      console.log(state);
    } catch (error) {
      console.error(error);
    }

  }
}
