import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card'; // For displaying cards
import { MatTooltipModule } from '@angular/material/tooltip';
import {RoundEndPlayerSummaryDto} from '../../../../core/models/roundEndPlayerSummaryDto';
import {CardDto} from '../../../../core/models/cardDto';
import {RoundEndSummaryDto} from '../../../../core/models/roundEndSummaryDto';
import {CardComponent} from '../card/card.component';
import {CARD_DETAILS_MAP} from '../card/CARD_DETAILS_MAP';

const getCardName = (value: number | undefined): string => {
  if (value === undefined) return '?';
  return CARD_DETAILS_MAP[value]?.name ?? '?';
};

@Component({
  selector: 'app-round-summary-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
    MatListModule,
    MatIconModule,
    MatCardModule,
    MatTooltipModule,
    CardComponent // To display the final held card
  ],
  templateUrl: './round-summary-dialog.component.html',
  styleUrls: ['./round-summary-dialog.component.scss']
})
export class RoundSummaryDialogComponent implements OnInit {

  public data: RoundEndSummaryDto = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<RoundSummaryDialogComponent>);

  winnerName: string | null = null;

  constructor() { }

  ngOnInit(): void {
    if (this.data.winnerPlayerId) {
      this.winnerName = this.data.playerSummaries.find(p => p.playerId === this.data.winnerPlayerId)?.playerName ?? null;
    }
  }

  // Helper to get card name for display in the template
  getCardNameFromValue(value: number | undefined): string {
    return getCardName(value);
  }

  // Helper to create CardDto for the discard pile display in the template
  // (since discardPileValues are just numbers)
  getDiscardCardDto(typeValue: number, playerSummary: RoundEndPlayerSummaryDto, index: number): CardDto {
    return {
      id: `${playerSummary.playerId}_discard_summary_${index}_${typeValue}`, // Unique ID for trackBy
      type: typeValue
    };
  }

  onDismiss(): void {
    this.dialogRef.close();
  }

  trackPlayerSummary(index: number, item: RoundEndPlayerSummaryDto): string {
    return item.playerId;
  }
  trackCardValue(index: number, item: number): number {
    return index; // Or item if values are unique in discard
  }
}
