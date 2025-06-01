import { CommonModule } from '@angular/common';
import {Component, EventEmitter, inject, OnInit, Output, ViewChild, OnDestroy} from '@angular/core';
import {MatListModule} from '@angular/material/list';
import {MatIconModule} from '@angular/material/icon';
import {MatDividerModule} from '@angular/material/divider';
import {MatButtonModule} from '@angular/material/button';
import {MatTabsModule, MatTabGroup} from '@angular/material/tabs';
import {CardReferenceService} from '../../../core/services/card-reference.service';
import {CardReferenceItem} from '../../../core/models/cardReferenceItem';
import {GameLogComponent} from '../../../features/game/components/game-log/game-log.component';
import { UiInteractionService, ScrollToCardReferenceRequest } from '../../../core/services/ui-interaction-service.service';
import { Subscription } from 'rxjs';
import { ElementRef } from '@angular/core';

@Component({
  selector: 'app-card-reference-sheet',
  standalone: true,
  imports: [
    CommonModule,
    MatListModule,
    MatIconModule,
    MatDividerModule,
    MatButtonModule,
    MatTabsModule,
    GameLogComponent
  ],
  templateUrl: './card-reference-sheet.component.html',
  styleUrls: ['./card-reference-sheet.component.scss']
})
export class CardReferenceSheetComponent implements OnInit, OnDestroy {
  private cardReferenceService = inject(CardReferenceService);
  private uiInteractionService = inject(UiInteractionService);
  private elementRef = inject(ElementRef);

  @ViewChild('tabGroup') tabGroup!: MatTabGroup;
  @Output() closeClicked = new EventEmitter<void>();

  cardReferences: CardReferenceItem[] = [];
  private scrollSubscription: Subscription | undefined;

  ngOnInit(): void {
    this.cardReferences = this.cardReferenceService.getAllCardReferences();
    this.scrollSubscription = this.uiInteractionService.scrollToCardReference$
      .subscribe((request: ScrollToCardReferenceRequest) => {
        this.scrollToCard(request.rank);
      });
  }

  ngOnDestroy(): void {
    this.scrollSubscription?.unsubscribe();
  }

  private scrollToCard(rank: number): void {
    if (this.tabGroup) {
      this.tabGroup.selectedIndex = 0; // Ensure 'Cards' tab is active
      // Use setTimeout to allow the tab content to render before scrolling
      setTimeout(() => {
        const cardElement = this.elementRef.nativeElement.querySelector(`#card-ref-${rank}`);
        if (cardElement) {
          cardElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        } else {
          console.warn(`CardReferenceSheet: Could not find element for card rank ${rank}`);
        }
      }, 0);
    }
  }

  onClose(): void {
    this.closeClicked.emit();
  }

  // Method to programmatically select a tab
  selectCardReferenceTab(): void {
    this.tabGroup.selectedIndex = 0;
  }

  // Helper for ngFor trackBy
  trackByRank(index: number, item: CardReferenceItem): number {
    return item.rank;
  }
}
