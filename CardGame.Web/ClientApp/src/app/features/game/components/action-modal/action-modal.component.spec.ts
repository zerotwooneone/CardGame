import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ActionModalComponent } from './action-modal.component';

describe('ActionModalComponent', () => {
  let component: ActionModalComponent;
  let fixture: ComponentFixture<ActionModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActionModalComponent],
      providers: [
        { provide: MatDialogRef, useValue: {} },
        { provide: MAT_DIALOG_DATA, useValue: {} }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ActionModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
