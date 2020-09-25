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
  gameIdX: string;
  constructor(private commonStateFactory: CommonStateFactoryService,
              private clientFactory: ClientFactoryService,
              private clientRouter: ClientRouterService) { }

  async ngOnInit(): Promise<void> {
    this.clientRouter.init();
  }

  async connect(gameId: string) {
    this.gameIdX = gameId;
    try {
      const c = await this.clientFactory.Create({ GameId: gameId });
      console.log('created client');
      const state = await c.State.pipe(first(), timeout(100)).toPromise();
      console.log(state);
    } catch (error) {
      console.error(error);
    }
  }
}
