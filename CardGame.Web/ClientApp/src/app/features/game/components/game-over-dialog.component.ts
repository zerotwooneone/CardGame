import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';

// Define the expected data structure for the dialog
export interface GameOverDialogData {
  winnerId: string | null; // Can be null if the game ends without a single winner (e.g., draw)
  winnerName?: string;    // Optional: The name of the winning player
}

@Component({
  selector: 'app-game-over-dialog',
  template: `
    <h2 mat-dialog-title>Game Over!</h2>
    <mat-dialog-content>
      <p *ngIf="data.winnerName">Congratulations, {{ data.winnerName }}!</p>
      <p *ngIf="!data.winnerName && data.winnerId">Player with ID {{ data.winnerId }} has won!</p>
      <p *ngIf="!data.winnerName && !data.winnerId">The game has concluded.</p>
      <!-- Can add scores or other details later -->
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="closeAndNavigate()">Back to Lobby</button>
    </mat-dialog-actions>
  `,
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule
  ]
})
export class GameOverDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<GameOverDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GameOverDialogData,
    private router: Router
  ) {}

  closeAndNavigate(): void {
    this.dialogRef.close();
    this.router.navigate(['/lobby']); // Or some other appropriate route
  }
}
