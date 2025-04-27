import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest } from '../../../core/models/loginRequest';

@Component({
  selector: 'app-login',
  standalone: true, // Mark as standalone
  imports: [ // Import necessary modules directly
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  rememberUsername = true; // Default to true

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // Key for local storage
  private readonly USERNAME_STORAGE_KEY = 'lastUsername';

  constructor() {
    // Initialize form group
    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(8)]],
      password: ['', Validators.required], // Add password complexity validator if desired on client-side too
      rememberUser: [true] // Control for the checkbox
    });
  }

  ngOnInit(): void {
    // Load saved username on init
    const savedUsername = localStorage.getItem(this.USERNAME_STORAGE_KEY);
    this.rememberUsername = !!savedUsername; // Checkbox reflects if username was saved
    this.loginForm.patchValue({
      username: savedUsername || '',
      rememberUser: this.rememberUsername
    });

    // Subscribe to checkbox changes to update local state
    this.loginForm.get('rememberUser')?.valueChanges.subscribe(value => {
      this.rememberUsername = value;
      if (!value) {
        // If unchecked, remove saved username immediately
        localStorage.removeItem(this.USERNAME_STORAGE_KEY);
      } else {
        // If checked, save current username (if any)
        const currentUsername = this.loginForm.get('username')?.value;
        if (currentUsername) {
          localStorage.setItem(this.USERNAME_STORAGE_KEY, currentUsername);
        }
      }
    });

    // Save username if remember is checked and username changes
    this.loginForm.get('username')?.valueChanges.subscribe(username => {
      if (this.rememberUsername && username) {
        localStorage.setItem(this.USERNAME_STORAGE_KEY, username);
      }
    });
  }

  onSubmit(): void {
    this.errorMessage = null; // Clear previous errors
    if (this.loginForm.invalid) {
      this.errorMessage = 'Please fill in both username and password.';
      return;
    }

    this.isLoading = true;

    const credentials: LoginRequest = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password
    };

    // Save username if 'remember' is checked
    if (this.rememberUsername) {
      localStorage.setItem(this.USERNAME_STORAGE_KEY, credentials.username);
    } else {
      localStorage.removeItem(this.USERNAME_STORAGE_KEY);
    }

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open(`Welcome ${response.username}!`, 'Close', { duration: 3000 });
        // Navigate to a protected route after successful login (e.g., game lobby or dashboard)
        this.router.navigate(['/lobby']); // Adjust route as needed
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Login failed. Please check credentials.'; // Display error from service
        this.snackBar.open(this.errorMessage ?? 'Login failed. Please check credentials.', 'Close', { duration: 5000 });
      }
    });
  }
}
