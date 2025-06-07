import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RoundSummaryDialogComponent } from './round-summary-dialog.component';
import { RoundEndSummaryDto } from '../../models/roundEndSummaryDto';
import { CardRank } from '../../models/cardRank';

describe('RoundSummaryDialogComponent', () => {
  let component: RoundSummaryDialogComponent;
  let fixture: ComponentFixture<RoundSummaryDialogComponent>;
  const mockDialogData: RoundEndSummaryDto = {
    reason: 'Highest card value',
    winnerPlayerId: '1',
    playerSummaries: [
      {
        playerId: '1',
        playerName: 'Player 1',
        tokensWon: 1,
        cardsHeld: [{ rank: CardRank.Guard, appearanceId: 'guard_1.png' }],
      },
      {
        playerId: '2',
        playerName: 'Player 2',
        tokensWon: 0,
        cardsHeld: [{ rank: CardRank.Prince, appearanceId: 'prince_5.png' }],
      },
    ],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RoundSummaryDialogComponent,
        HttpClientTestingModule
      ],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: mockDialogData },
        { provide: MatDialogRef, useValue: {} }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RoundSummaryDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
