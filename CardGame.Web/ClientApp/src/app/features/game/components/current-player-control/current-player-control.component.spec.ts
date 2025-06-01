import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CurrentPlayerControlComponent } from './current-player-control.component';

describe('CurrentPlayerControlComponent', () => {
  let component: CurrentPlayerControlComponent;
  let fixture: ComponentFixture<CurrentPlayerControlComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CurrentPlayerControlComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CurrentPlayerControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
