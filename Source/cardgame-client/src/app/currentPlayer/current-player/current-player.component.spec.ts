import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CurrentPlayerComponent } from './current-player.component';
import { CurrentPlayerModel } from '../current-player-model';

describe('CurrentPlayerComponent', () => {
  let component: CurrentPlayerComponent;
  let fixture: ComponentFixture<CurrentPlayerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CurrentPlayerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CurrentPlayerComponent);
    component = fixture.componentInstance;

    component.player = createMockPlayerModel();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

function createMockPlayerModel(): CurrentPlayerModel {
  return new CurrentPlayerModel('some test id');
}
