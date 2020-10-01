import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayChoiceComponent } from './play-choice.component';

describe('PlayChoiceComponent', () => {
  let component: PlayChoiceComponent;
  let fixture: ComponentFixture<PlayChoiceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlayChoiceComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlayChoiceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
