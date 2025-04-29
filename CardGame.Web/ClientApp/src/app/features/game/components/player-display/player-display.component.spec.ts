import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerDisplayComponent } from './player-display.component';

describe('PlayerDisplayComponent', () => {
  let component: PlayerDisplayComponent;
  let fixture: ComponentFixture<PlayerDisplayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerDisplayComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlayerDisplayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
