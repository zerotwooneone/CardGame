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
  /** Player data to display. PlayerHandInfoDto has playedCardTypes: number[] */
  @Input() playerData?: PlayerHandInfoDto | null;

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
    return Array(Math.max(0, count)).fill(0).map((x, i) => i);
  }

  // Helper to create CardDto objects for the discard pile display
  // Uses the numeric type value from playerData.playedCardTypes
  get discardPileCards(): CardDto[] {
    return (this.playerData?.playedCardTypes ?? []).map((typeValue, index) => ({
      // Generate a pseudo-stable ID based on player and index for *ngFor trackBy
      id: `${this.playerData?.playerId}_discard_${index}_${typeValue}`, // Include typeValue for more stability
      type: typeValue // Assign the numeric type value directly
    }));
  }

  // --- TrackBy Functions ---
  trackByIndex(index: number, item: any): number {
    return index;
  }

  trackCardById(index: number, item: CardDto): string {
    return item.id; // Use unique card ID for tracking
  }

  trackPlayerById(index: number, item: PlayerHandInfoDto): string {
    return item.playerId;
  }
  // --- End TrackBy Functions ---

}
