import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { CardReferenceSheetComponent } from './card-reference-sheet.component';

describe('CardReferenceSheetComponent', () => {
  let component: CardReferenceSheetComponent;
  let fixture: ComponentFixture<CardReferenceSheetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardReferenceSheetComponent, HttpClientTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CardReferenceSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
