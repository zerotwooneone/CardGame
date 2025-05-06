import { CommonModule } from '@angular/common';
import {Component, computed, inject, OnInit, signal, WritableSignal} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MAT_DIALOG_DATA, MatDialogModule, MatDialogRef} from '@angular/material/dialog';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatListModule} from '@angular/material/list';
import {MatIconModule} from '@angular/material/icon';
import {MatSnackBar, MatSnackBarModule} from '@angular/material/snack-bar';
import {CreateGameDialogData} from '../models/createGameDialogData';
import {CreateGameDialogResult} from '../models/createGameDialogResult';
import {FriendSelection} from '../models/friendSelection';
import {UserInfo} from '../../../core/models/userInfo';

@Component({
  selector: 'app-create-game-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatListModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './create-game-dialog.component.html',
  styleUrls: ['./create-game-dialog.component.scss']
})
export class CreateGameDialogComponent implements OnInit {

  // Inject dependencies
  public data: CreateGameDialogData = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<CreateGameDialogComponent, CreateGameDialogResult>);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  createGameForm: FormGroup;
  knownFriendSelections: WritableSignal<FriendSelection[]> = signal([]);
  newlyValidatedFriends: WritableSignal<UserInfo[]> = signal([]);

  // Compute total selected opponents
  totalSelectedOpponents = computed(() => {
    const selectedKnown = this.knownFriendSelections().filter(f => f.selected).length;
    const selectedNew = this.newlyValidatedFriends().length;
    return selectedKnown + selectedNew;
  });

  // Compute validity based on opponent count (1 to 3 opponents allowed)
  isValidOpponentCount = computed(() => {
    const count = this.totalSelectedOpponents();
    return count >= 1 && count <= 3;
  });

  constructor() {
    this.createGameForm = this.fb.group({
      friendCodeInput: [''], // Text area for pasting/entering new codes
      // Known friends selection will be managed by the signal/template interaction
    });
  }

  ngOnInit(): void {
    // Initialize selectable list from injected data
    this.knownFriendSelections.set(
      this.data.knownFriends.map(f => ({ ...f, selected: false }))
    );
  }

  toggleKnownFriendSelection(friend: FriendSelection): void {
    this.knownFriendSelections.update(current =>
      current.map(f =>
        f.playerId === friend.playerId ? { ...f, selected: !f.selected } : f
      )
    );
  }

  validateAndAddFriendCodes(): void {
    const inputControl = this.createGameForm.get('friendCodeInput');
    if (!inputControl || !inputControl.value?.trim()) {
      this.showError("Input field is empty.");
      return;
    }

    const rawInput = inputControl.value.trim();
    // Attempt to parse as JSON array or single JSON object
    let potentialCodes: string[] = [];
    try {
      // Try parsing as array first
      const parsedArray = JSON.parse(rawInput);
      if (Array.isArray(parsedArray)) {
        // Assume array of strings (friend codes) or objects
        potentialCodes = parsedArray.map(item => typeof item === 'string' ? item : JSON.stringify(item));
      } else if (typeof parsedArray === 'object') {
        // Assume single object pasted
        potentialCodes.push(rawInput);
      } else {
        throw new Error("Input is not valid JSON object or array.");
      }
    } catch (e) {
      // If parsing as array/object fails, try splitting by newline assuming multiple codes pasted
      potentialCodes = rawInput.split(/[\n\r]+/).map((s: string) => s.trim()).filter((s: string) => s.length > 0);
    }


    let addedCount = 0;
    let invalidCount = 0;
    const newlyAdded: UserInfo[] = [];

    potentialCodes.forEach(codeStr => {
      try {
        const userInfo: UserInfo = JSON.parse(codeStr);
        // Basic validation
        if (userInfo && typeof userInfo.playerId === 'string' && userInfo.playerId && typeof userInfo.username === 'string' && userInfo.username) {
          // Check if not already known or newly added
          if (!this.knownFriendSelections().some(f => f.playerId === userInfo.playerId) &&
            !this.newlyValidatedFriends().some(f => f.playerId === userInfo.playerId))
          {
            newlyAdded.push(userInfo);
            addedCount++;
          } else {
            // Already known or just added
            console.log(`Friend code for ${userInfo.username} already known/added.`);
          }
        } else {
          invalidCount++;
        }
      } catch (e) {
        invalidCount++;
        console.warn("Could not parse potential friend code:", codeStr, e);
      }
    });

    // Update the signal with newly validated friends
    if (newlyAdded.length > 0) {
      this.newlyValidatedFriends.update(current => [...current, ...newlyAdded]);
    }

    // Provide feedback
    let message = '';
    if (addedCount > 0) message += `${addedCount} new friend(s) added. `;
    if (invalidCount > 0) message += `${invalidCount} invalid code(s) ignored.`;
    if (message) {
      this.showSuccess(message.trim());
    } else {
      this.showError("No new valid friend codes found in input, or friends already known.");
    }

    // Clear the input field
    inputControl.reset();
  }

  removeNewlyAddedFriend(friend: UserInfo): void {
    this.newlyValidatedFriends.update(current => current.filter(f => f.playerId !== friend.playerId));
  }

  onConfirm(): void {
    // Combine selected known friends and newly added friends
    const selectedKnown = this.knownFriendSelections()
      .filter(f => f.selected)
      .map(f => ({ playerId: f.playerId, username: f.username })); // Map back to UserInfo

    const allSelectedOpponents = [...selectedKnown, ...this.newlyValidatedFriends()];

    // Use the computed property for final count validation
    if (!this.isValidOpponentCount()) {
      this.showError(`Please select between 1 and 3 opponents (currently ${this.totalSelectedOpponents()}).`);
      return;
    }

    const result: CreateGameDialogResult = {
      selectedOpponentIds: allSelectedOpponents.map(f => f.playerId),
      // Send back the JSON strings for newly added friends so parent can store them
      newlyValidatedFriendCodes: this.newlyValidatedFriends().map(f => JSON.stringify(f))
    };
    this.dialogRef.close(result);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'OK', { duration: 3000 });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 4000 });
  }
}
