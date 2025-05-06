import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {MatToolbar} from '@angular/material/toolbar';
import {UserWidgetComponent} from './shared/components/user-widget/user-widget.component';

@Component({
  selector: 'cgc-root',
  imports: [RouterOutlet, MatToolbar, UserWidgetComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'Card Game';
}
