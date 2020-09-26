import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { GameBoardComponent } from './gameBoard/game-board/game-board.component';
import { OtherPlayerComponent } from './otherPlayer/other-player/other-player.component';
import { CurrentPlayerComponent } from './currentPlayer/current-player/current-player.component';
import { PlayableCardComponent } from './card/playable-card/playable-card.component';
import { PlayerService } from './player/player.service';
import { APIInterceptor } from './client/APIInterceptor';

@NgModule({
  declarations: [
    AppComponent,
    GameBoardComponent,
    OtherPlayerComponent,
    CurrentPlayerComponent,
    PlayableCardComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production })
  ],
  providers: [
    {
      provide: 'IPlayerService',
      useClass: PlayerService
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: APIInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
