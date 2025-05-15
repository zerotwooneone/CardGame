import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { UserWidgetComponent } from './user-widget.component';

describe('UserWidgetComponent', () => {
  let component: UserWidgetComponent;
  let fixture: ComponentFixture<UserWidgetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserWidgetComponent, HttpClientTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserWidgetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
