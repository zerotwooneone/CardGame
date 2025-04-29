import {Component, inject, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {MAT_DIALOG_DATA, MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import {MatListModule} from '@angular/material/list';
import {ActionModalData} from '../../actionModalData';
import {ActionModalResult} from '../../actionModalResult';
import {MatSelectModule} from '@angular/material/select';
import {MatRadioModule} from '@angular/material/radio';

@Component({
  selector: 'app-action-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDialogModule,
    MatDividerModule,
    MatListModule,
    MatRadioModule
  ],
  templateUrl: './action-modal.component.html',
  styleUrls: ['./action-modal.component.scss']
})
export class ActionModalComponent implements OnInit {

  // Inject dialog data and reference
  public data: ActionModalData = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<ActionModalComponent, ActionModalResult>); // Specify result type
  private fb = inject(FormBuilder);

  actionForm: FormGroup;
  filteredPlayers: { id: string; name: string; isProtected: boolean }[] = [];
  filteredCardTypes: { value: number; name: string }[] = [];

  constructor() {
    // Initialize form - controls added dynamically in ngOnInit
    this.actionForm = this.fb.group({});
  }

  ngOnInit(): void {
    this.setupFormControls();
    this.filterOptions();
  }

  private setupFormControls(): void {
    if (this.data.actionType === 'select-player') {
      this.actionForm.addControl('selectedPlayerId', this.fb.control(null, Validators.required));
    } else if (this.data.actionType === 'guess-card') {
      this.actionForm.addControl('selectedCardTypeValue', this.fb.control(null, Validators.required));
    }
  }

  private filterOptions(): void {
    if (this.data.actionType === 'select-player' && this.data.availablePlayers) {
      // Filter out the current player and protected players
      this.filteredPlayers = this.data.availablePlayers.filter(p =>
        p.id !== this.data.currentPlayerId && !p.isProtected
      );
      // If no valid targets, disable submit? Or handle in parent component before opening?
    } else if (this.data.actionType === 'guess-card' && this.data.availableCardTypes) {
      // Filter out the excluded card type (e.g., Guard)
      this.filteredCardTypes = this.data.availableCardTypes.filter(ct =>
        ct.value !== this.data.excludeCardTypeValue
      );
    }
  }

  onConfirm(): void {
    if (this.actionForm.valid) {
      const result: ActionModalResult = {};
      if (this.data.actionType === 'select-player') {
        result.selectedPlayerId = this.actionForm.value.selectedPlayerId;
      } else if (this.data.actionType === 'guess-card') {
        result.selectedCardTypeValue = this.actionForm.value.selectedCardTypeValue;
      }
      this.dialogRef.close(result); // Close dialog and return the result
    }
  }

  onCancel(): void {
    this.dialogRef.close(); // Close without returning data
  }
}
