import { Component, inject, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common'; // Import CommonModule
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import {UserWidgetComponent} from './shared/components/user-widget/user-widget.component';
import {CardReferenceSheetComponent} from './shared/components/card-reference-sheet/card-reference-sheet.component';

@Component({
  selector: 'cgc-root',
  standalone: true,
  imports: [
    CommonModule, // Add CommonModule
    RouterOutlet,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatSidenavModule, // Add MatSidenavModule
    UserWidgetComponent,
    CardReferenceSheetComponent // Add CardReferenceSheetComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'CardGame'; // Or your app title

  @ViewChild('cardRefSidenav') cardRefSidenav!: MatSidenav;

  toggleCardReference(): void {
    this.cardRefSidenav.toggle();
  }

  // Optional: Method to scroll to a specific card in the reference sheet
  // This would be called from other components (e.g., CardComponent) via a service.
  // For now, just the toggle is implemented here.
  scrollToCardReference(cardRank: number): void {
    this.cardRefSidenav.open().then(() => {
      const element = document.getElementById(`card-ref-${cardRank}`);
      element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  }
}
