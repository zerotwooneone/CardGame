import {ChangeDetectionStrategy, Component, computed, inject, signal, WritableSignal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MatIconModule} from '@angular/material/icon';
import {MatButtonModule} from '@angular/material/button';
import {MatMenuModule} from '@angular/material/menu';
import {MatDividerModule} from '@angular/material/divider';
import {MatSnackBar, MatSnackBarModule} from '@angular/material/snack-bar';
import {AuthService} from '@features/auth/services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { UserInfo } from '@features/auth/models/userInfo';
import {ClipboardModule, Clipboard} from '@angular/cdk/clipboard';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-user-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
    MatSnackBarModule,
    ClipboardModule,
    RouterModule
  ],
  templateUrl: './user-widget.component.html',
  styleUrls: ['./user-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserWidgetComponent {
  // Inject services
  private authService = inject(AuthService);
  private clipboard = inject(Clipboard);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  // Get logged-in status and username (if available)
  isLoggedIn = this.authService.isLoggedIn; // Use the signal directly
  // Get username from localStorage (or ideally a user profile service/signal later)
  username = computed(() => {
    if (this.authService.isLoggedIn() && typeof localStorage !== 'undefined') {
      return localStorage.getItem('currentUsername') ?? 'User';
    }
    return 'User';
  });

  // Local state for the friend code
  friendCode: WritableSignal<string | null> = signal(null);

  generateAndCopyFriendCode(): void {
    if (typeof localStorage === 'undefined') {
      this.showError('Cannot generate code outside browser.');
      return;
    }
    const playerId = this.authService.getCurrentPlayerId();
    const currentUsername = localStorage.getItem('currentUsername'); // Get username associated with session

    if (playerId && currentUsername) {
      const userInfo: UserInfo = { username: currentUsername, playerId: playerId };
      const code = JSON.stringify(userInfo);
      this.friendCode.set(code); // Display the code

      // Copy to clipboard
      const success = this.clipboard.copy(code);
      if (success) {
        this.showSuccess('Friend Code copied to clipboard!');
      } else {
        this.showError('Failed to copy Friend Code automatically.');
      }
    } else {
      this.showError('Could not retrieve user information to generate code.');
    }
  }

  copyCodeAgain(): void {
    const code = this.friendCode();
    if (code) {
      const success = this.clipboard.copy(code);
      if (success) {
        this.showSuccess('Friend Code copied again!');
      } else {
        this.showError('Failed to copy Friend Code automatically.');
      }
    }
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.friendCode.set(null); // Clear code on logout
        this.router.navigate(['/auth/login']); // Redirect
      },
      error: (err) => this.showError(`Logout failed: ${err.message}`)
    });
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'OK', { duration: 3000, panelClass: 'snackbar-success' });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 4000, panelClass: 'snackbar-error' });
  }
}
