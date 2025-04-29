import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostBinding, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge'; // For token count
import { MatTooltipModule } from '@angular/material/tooltip'; // For tooltips

// Import shared models and components
import { CardComponent } from '../card/card.component';
import {PlayerHandInfoDto} from '../../../../core/models/playerHandInfoDto'; // Import CardComponent
import { CardDto } from '../../../../core/models/cardDto';

@Component({
  selector: 'app-player-display',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatBadgeModule,
    MatTooltipModule,
    CardComponent // Import CardComponent here
  ],
  templateUrl: './player-display.component.html',
  styleUrls: ['./player-display.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlayerDisplayComponent {
  /** Player data to display. Can be SpectatorPlayerDto or PlayerHandInfoDto. */
  @Input() playerData?: PlayerHandInfoDto | null; // Use the DTO that includes necessary info

  /** Is it currently this player's turn? */
  @Input() isCurrentTurn: boolean = false;

  /** Is this the player currently using the application? */
  @Input() isCurrentUser: boolean = false;

  /** Can this player currently be targeted by an action? */
  @Input() isTargetable: boolean = false;

  /** Is this player the currently selected target? */
  @Input() isSelectedTarget: boolean = false;

  /** Emits the player's ID when this component is clicked and is targetable. */
  @Output() playerClicked = new EventEmitter<string>();

  // --- Host Bindings for dynamic styling ---
  @HostBinding('class.current-turn') get turnClass() { return this.isCurrentTurn; }
  @HostBinding('class.eliminated') get eliminatedClass() { return this.playerData?.status === 'Eliminated'; }
  @HostBinding('class.protected') get protectedClass() { return this.playerData?.isProtected; }
  @HostBinding('class.targetable') get targetableClass() { return this.isTargetable; }
  @HostBinding('class.selected-target') get selectedTargetClass() { return this.isSelectedTarget; }
  @HostBinding('class.current-user') get currentUserClass() { return this.isCurrentUser; }

  // --- Host Listener for clicks ---
  @HostListener('click')
  onClick() {
    if (this.isTargetable && this.playerData?.playerId) {
      this.playerClicked.emit(this.playerData.playerId);
    }
  }

  // Helper to create an array for iterating card placeholders in the template
  get handPlaceholders(): number[] {
    const count = this.playerData?.handCardCount ?? 0;
    return Array(count).fill(0).map((x, i) => i);
  }

  // Helper to create CardDto objects for the discard pile display
  // We only have CardType names, so we create dummy CardDtos
  get discardPileCards(): CardDto[] {
    return (this.playerData?.playedCardTypes ?? []).map((typeName, index) => ({
      // Generate a pseudo-stable ID based on player and index for *ngFor trackBy
      id: `${this.playerData?.playerId}_discard_${index}`,
      type: typeName
      // AppearanceId is not available from this DTO, use default or map if needed
    }));
  }

  /**
   * TrackBy function for loops based on index.
   * @param index The index of the item.
   * @param item The item itself (unused).
   * @returns The index.
   */
  trackByIndex(index: number, item: any): number { // Added method
    return index;
  }

  /**
   * TrackBy function for loops over CardDto objects.
   * @param index The index of the item.
   * @param item The CardDto item.
   * @returns The unique card ID.
   */
  trackCardById(index: number, item: CardDto): string {
    return item.id; // Use unique card ID for tracking
  }

}
