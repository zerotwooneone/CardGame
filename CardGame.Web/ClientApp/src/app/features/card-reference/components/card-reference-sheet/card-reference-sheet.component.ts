import { CommonModule } from '@angular/common';
import {Component, EventEmitter, inject, OnInit, Output, ViewChild, OnDestroy, computed } from '@angular/core';
import {MatListModule} from '@angular/material/list';
import {MatIconModule} from '@angular/material/icon';
import {MatDividerModule} from '@angular/material/divider';
import {MatButtonModule} from '@angular/material/button';
import {MatTabsModule, MatTabGroup} from '@angular/material/tabs';
import { CardReferenceService } from '../../services/card-reference.service';
import { CardReferenceItem } from '../../models/cardReferenceItem';
import { GameLogComponent } from '../../../game/components/game-log/game-log.component';
import { UiInteractionService, ScrollToCardReferenceRequest } from '../../services/ui-interaction-service.service';
import { Subscription } from 'rxjs';
import { ElementRef } from '@angular/core';
import { CardDisplayComponent } from '../../../game/components/card-display/card-display.component';
import { DeckService } from '../../../game/services/deck.service';

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
    GameLogComponent,
    CardDisplayComponent
  ],
  templateUrl: './card-reference-sheet.component.html',
  styleUrls: ['./card-reference-sheet.component.scss']
})
export class CardReferenceSheetComponent implements OnInit, OnDestroy {
  private cardReferenceService = inject(CardReferenceService);
  private uiInteractionService = inject(UiInteractionService);
  private elementRef = inject(ElementRef);
  private deckService = inject(DeckService);

  @ViewChild('tabGroup') tabGroup!: MatTabGroup;
  @Output() closeClicked = new EventEmitter<void>();

  cardReferences: CardReferenceItem[] = [];
  private scrollSubscription: Subscription | undefined;

  public cardReferencesForDisplay = computed(() => {
    const staticRefs = this.cardReferences;
    const currentDeck = this.deckService.deckDefinition();

    if (!currentDeck || !currentDeck.cards || currentDeck.cards.length === 0) {
      // If no deck or deck has no cards, return static refs with a single placeholder appearance
      return staticRefs.map(ref => ({
        ...ref,
        appearances: [{ rank: ref.rank, appearanceId: '' }] 
      }));
    }

    return staticRefs.map(ref => {
      const allMatchingCardsFromDeck = currentDeck.cards
        .filter(deckCard => deckCard.rank === ref.rank);

      // Get unique appearanceIds for the current rank
      const uniqueAppearanceIds = Array.from(new Set(allMatchingCardsFromDeck.map(c => c.appearanceId)));

      const appearances = uniqueAppearanceIds.map(appId => {
        // Find the first card that matches this rank and unique appearanceId to get its details
        // (rank should be the same, but this confirms we're linking correctly)
        const originalCard = allMatchingCardsFromDeck.find(c => c.appearanceId === appId);
        return {
          rank: originalCard ? originalCard.rank : ref.rank, // Fallback to ref.rank if something is off
          appearanceId: appId
        };
      });

      return {
        ...ref,
        // If no specific appearances found for a rank, show one placeholder.
        // Otherwise, show all unique found appearances.
        appearances: appearances.length > 0 ? appearances : [{ rank: ref.rank, appearanceId: '' }]
      };
    });
  });

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
          cardElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
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
  trackByRank(index: number, item: CardReferenceItem & { appearances: any[] }): number {
    return item.rank;
  }
}
