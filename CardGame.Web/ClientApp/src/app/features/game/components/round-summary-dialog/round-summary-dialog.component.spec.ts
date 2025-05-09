import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RoundSummaryDialogComponent } from './round-summary-dialog.component';

describe('RoundSummaryDialogComponent', () => {
  let component: RoundSummaryDialogComponent;
  let fixture: ComponentFixture<RoundSummaryDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RoundSummaryDialogComponent]
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
