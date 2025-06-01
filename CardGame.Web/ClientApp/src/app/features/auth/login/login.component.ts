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

import { AuthService } from '../services/auth.service';
import { LoginRequest } from '../models/loginRequest';

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
  styleUrls: ['./login.component.scss'] // Use styleUrls for component-specific styles, changed to .scss
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  rememberUsername = true; // Default to true

  // Inject services using inject() function (preferred in Angular 14+)
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // Key for local storage
  private readonly USERNAME_STORAGE_KEY = 'lastUsername';
  private readonly REMEMBER_USERNAME_KEY = 'rememberUsernamePref';

  // Password complexity regex (matches backend)
  private passwordComplexityRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$/;

  constructor() {
    // Initialize form group
    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(1)]],
      // Add Validators.pattern for password complexity
      password: ['', [
        Validators.required,
        Validators.pattern(this.passwordComplexityRegex) // Added pattern validator
      ]],
      rememberUser: [true] // Control for the checkbox
    });
  }

  ngOnInit(): void {
    // Load saved username and preference on init (browser only)
    if (typeof localStorage !== 'undefined') { // Check if localStorage is available
      const savedUsername = localStorage.getItem(this.USERNAME_STORAGE_KEY);
      this.rememberUsername = localStorage.getItem(this.REMEMBER_USERNAME_KEY) !== 'false'; // Default true
      this.loginForm.patchValue({
        username: savedUsername || '',
        rememberUser: this.rememberUsername
      });
    } else {
      this.rememberUsername = true; // Default if no localStorage
      this.loginForm.patchValue({ rememberUser: true });
    }


    // Subscribe to checkbox changes to update preference
    this.loginForm.get('rememberUser')?.valueChanges.subscribe(value => {
      this.rememberUsername = value;
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem(this.REMEMBER_USERNAME_KEY, value.toString());
        if (!value) {
          localStorage.removeItem(this.USERNAME_STORAGE_KEY); // Clear saved username if unchecked
        } else {
          // If checked, save current username (if any)
          const currentUsername = this.loginForm.get('username')?.value;
          if (currentUsername) {
            localStorage.setItem(this.USERNAME_STORAGE_KEY, currentUsername);
          }
        }
      }
    });

    // Save username if remember is checked and username changes
    this.loginForm.get('username')?.valueChanges.subscribe(username => {
      if (this.rememberUsername && username && typeof localStorage !== 'undefined') {
        localStorage.setItem(this.USERNAME_STORAGE_KEY, username);
      }
    });
  }

  onSubmit(): void {
    this.errorMessage = null; // Clear previous errors

    // Mark fields as touched to show validation errors immediately on submit attempt
    this.loginForm.markAllAsTouched();

    if (this.loginForm.invalid) {
      // Check specific password error for better message
      if (this.loginForm.get('password')?.hasError('required')) {
        this.errorMessage = 'Password is required.';
      } else if (this.loginForm.get('password')?.hasError('pattern')) {
        this.errorMessage = 'Password must be 8+ chars with uppercase, lowercase, number, and special character.';
      } else if (this.loginForm.get('username')?.invalid) {
        this.errorMessage = 'Username is required.';
      } else {
        this.errorMessage = 'Please correct the errors in the form.';
      }
      return; // Stop submission if form is invalid
    }

    this.isLoading = true;

    const credentials: LoginRequest = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password
    };

    // Save username preference and potentially the username itself
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.REMEMBER_USERNAME_KEY, this.rememberUsername.toString());
      if (this.rememberUsername) {
        localStorage.setItem(this.USERNAME_STORAGE_KEY, credentials.username);
      } else {
        localStorage.removeItem(this.USERNAME_STORAGE_KEY);
      }
    }


    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.isLoading = false;
        // Check optional properties
        const username = response?.username ?? 'User';
        this.snackBar.open(`Welcome ${username}!`, 'Close', { duration: 3000 });
        // Navigate to a protected route after successful login (e.g., game lobby or dashboard)
        this.router.navigate(['/lobby']); // Adjust route as needed
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Login failed. Please check credentials.'; // Display error from service
        this.snackBar.open(this.errorMessage?? 'Login failed. Please check credentials.', 'Close', { duration: 5000 });
      }
    });
  }
}
