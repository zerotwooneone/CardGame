import { Component, OnInit, Inject, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatListModule } from '@angular/material/list';

import { CardDisplayComponent } from '../card-display/card-display.component';
import { RoundEndSummaryDto } from '../../../../core/models/roundEndSummaryDto';
import { RoundEndPlayerSummaryDto } from '../../../../core/models/roundEndPlayerSummaryDto';
import { CardDto } from '../../../../core/models/cardDto';

@Component({
  selector: 'app-round-summary-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
    MatIconModule,
    MatCardModule,
    MatTooltipModule,
    MatListModule,
    CardDisplayComponent
  ],
  templateUrl: './round-summary-dialog.component.html',
  styleUrls: ['./round-summary-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoundSummaryDialogComponent implements OnInit {
  public readonly data: RoundEndSummaryDto = inject(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<RoundSummaryDialogComponent>);

  public winnerNameDisplay: string | null = null;

  ngOnInit(): void {
    if (this.data) {
      if (this.data.winnerPlayerId && this.data.playerSummaries) {
        const winner = this.data.playerSummaries.find(p => p.playerId === this.data.winnerPlayerId);
        this.winnerNameDisplay = winner ? winner.playerName : 'Unknown Winner';
      } else {
        this.winnerNameDisplay = null; // Indicates a draw or no specific winner ID provided
      }
    } else {
      console.error('RoundSummaryDialogComponent: No data was provided to the dialog.');
      // Consider closing the dialog or displaying an error message if this.data can legitimately be null/undefined
    }
  }

  public onDismiss(): void {
    this.dialogRef.close();
  }

  public trackPlayerSummaryById(index: number, item: RoundEndPlayerSummaryDto): string {
    return item.playerId;
  }

  public trackCardByAppearanceId(index: number, card: CardDto): string {
    return card.appearanceId; 
  }
}
