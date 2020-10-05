import { Component, OnInit } from '@angular/core';
import { CommonStateFactoryService } from './commonState/common-state-factory.service';
import { ClientFactoryService } from './client/client-factory.service';
import { ClientRouterService } from './client/client-router.service';
import { FormControl, Validators } from '@angular/forms';
import { GameModelFactoryService } from './game/game-model-factory.service';
import { CurrentPlayerModelFactoryService } from './currentPlayer/current-player-model-factory.service';
import { GameModel } from './game/game-model';
import { CurrentPlayerModel } from './currentPlayer/current-player-model';

@Component({
  selector: 'cgc-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'cardgame-client';
  gameIdX: string;
  playerTokenControl: FormControl;
  gameTokenControl: FormControl;
  gameModel: GameModel;
  currentPlayer: CurrentPlayerModel;
  constructor(private readonly clientFactory: ClientFactoryService,
    private readonly clientRouter: ClientRouterService,
    private readonly commonStateFactory: CommonStateFactoryService,
    private readonly gameModelFactory: GameModelFactoryService,
    private readonly currentPlayerModelFactory: CurrentPlayerModelFactoryService) { }

  async ngOnInit(): Promise<void> {
    this.clientRouter.init();
    this.playerTokenControl = new FormControl('9b644228-6c7e-4caa-becf-89e093ee299f', Validators.required);
    this.gameTokenControl = new FormControl('96a8f4b0-7800-4c26-80b6-fd66f286140f', Validators.required);
  }

  async connect(gameId: string, playerId: string) {
    this.gameIdX = gameId;
    try {
      // we call this to open the signalR connection
      const c = await this.clientFactory.Create({ GameId: gameId });

      const commonState = await this.commonStateFactory.get(gameId);
      this.gameModel = await this.gameModelFactory.create(commonState, gameId);

      this.currentPlayer = await this.currentPlayerModelFactory
        .getById(playerId, gameId, this.gameModel);
    } catch (error) {
      console.error(error);
    }
  }
}
