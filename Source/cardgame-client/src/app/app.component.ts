import { Component, OnInit } from '@angular/core';
import { CommonStateFactoryService } from './commonState/common-state-factory.service';
import { ClientFactoryService } from './client/client-factory.service';
import { first, timeout } from 'rxjs/operators';
import { ClientRouterService } from './client/client-router.service';

@Component({
  selector: 'cgc-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'cardgame-client';
  gameId = '96a8f4b0-7800-4c26-80b6-fd66f286140f';

  constructor(private commonStateFactory: CommonStateFactoryService,
    private clientFactory: ClientFactoryService,
    private clientRouter: ClientRouterService) { }

  async ngOnInit(): Promise<void> {
    this.clientRouter.init();
    try {
      const commonState = await this.commonStateFactory.create(this.gameId);
    } catch (error) {
      console.error(error);
    }

    try {
      const c = await this.clientFactory.Create({ GameId: this.gameId });
      console.log('created client');
      const state = await c.State.pipe(first(), timeout(100)).toPromise();
      console.log(state);
    } catch (error) {
      console.error(error);
    }

  }
}
