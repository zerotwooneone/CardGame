import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CreateGameDialogComponent } from './create-game-dialog.component';

describe('CreateGameDialogComponent', () => {
  let component: CreateGameDialogComponent;
  let fixture: ComponentFixture<CreateGameDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateGameDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: {} },
        { provide: MAT_DIALOG_DATA, useValue: { knownFriends: [] } }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateGameDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
