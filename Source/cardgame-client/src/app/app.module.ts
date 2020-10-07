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
import { APIInterceptor } from './client/APIInterceptor';
import { PlayChoiceComponent } from './card/play-choice/play-choice.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { ScoreCardComponent } from './gameBoard/score-card/score-card.component';
import { MatCardModule } from '@angular/material/card';

@NgModule({
  declarations: [
    AppComponent,
    GameBoardComponent,
    OtherPlayerComponent,
    CurrentPlayerComponent,
    PlayableCardComponent,
    PlayChoiceComponent,
    ScoreCardComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production }),
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    MatBottomSheetModule,
    MatCardModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: APIInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
