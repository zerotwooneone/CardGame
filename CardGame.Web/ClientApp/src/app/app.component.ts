import {Component, inject, OnDestroy, OnInit, ViewChild} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common'; // Import CommonModule
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import {UserWidgetComponent} from '@features/auth/components/user-widget/user-widget.component';
import {CardReferenceSheetComponent} from '@features/card-reference/components/card-reference-sheet/card-reference-sheet.component';
import {UiInteractionService} from '@features/card-reference/services/ui-interaction-service.service';
import {Subject} from 'rxjs';
import {takeUntil} from 'rxjs/operators';

@Component({
  selector: 'cgc-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatSidenavModule,
    UserWidgetComponent,
    CardReferenceSheetComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'CardGame';

  @ViewChild('cardRefSidenav') cardRefSidenav!: MatSidenav;
  @ViewChild(CardReferenceSheetComponent) cardReferenceSheet!: CardReferenceSheetComponent;

  // Inject UiInteractionService
  private uiInteractionService = inject(UiInteractionService);
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // Subscribe to requests to scroll the card reference
    this.uiInteractionService.scrollToCardReference$
      .pipe(takeUntil(this.destroy$))
      .subscribe(request => {
        this.openAndScrollToCardReference(request.rank);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleCardReference(): void {
    this.cardRefSidenav.toggle();
  }

  /**
   * Opens the sidenav and attempts to scroll to the specified card rank
   * after a short delay to allow the sidenav to render.
   * @param cardRank The rank of the card to scroll to.
   */
  private openAndScrollToCardReference(cardRank: number): void {
    // Ensure the "Cards" tab is selected before opening/scrolling
    this.cardReferenceSheet?.selectCardReferenceTab(); // 0 is assumed to be the 'Cards' tab index

    this.cardRefSidenav.open().then((result) => {
      // The promise resolves when the open animation is complete ('open')
      // or if it was already open ('open').
      if (result === 'open' || this.cardRefSidenav.opened) {
        // Wait briefly for the sidenav content to be fully visible and layout complete,
        // especially if it was just opened.
        setTimeout(() => {
          const elementId = `card-ref-${cardRank}`;
          const element = document.getElementById(elementId);
          if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });
          } else {
            console.warn(`CardReferenceSheet: Element with ID '${elementId}' not found for scrolling.`);
          }
        }, 150); // Adjust delay if needed (e.g., 100-200ms)
      }
    });
  }
}
