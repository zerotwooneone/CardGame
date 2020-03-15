import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { GameBoardComponent } from './gameBoard/game-board/game-board.component';
import { OtherPlayerComponent } from './otherPlayer/other-player/other-player.component';
import { CurrentPlayerComponent } from './currentPlayer/current-player/current-player.component';

@NgModule({
  declarations: [
    AppComponent,
    GameBoardComponent,
    OtherPlayerComponent,
    CurrentPlayerComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production })
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
