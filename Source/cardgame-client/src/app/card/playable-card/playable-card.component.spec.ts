import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayableCardComponent } from './playable-card.component';

describe('PlayableCardComponent', () => {
  let component: PlayableCardComponent;
  let fixture: ComponentFixture<PlayableCardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlayableCardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlayableCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
