import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen bg-[var(--bg)] flex flex-col justify-center py-12 sm:px-6 lg:px-8 font-sans">
      <div class="sm:mx-auto sm:w-full sm:max-w-md text-center">
        <div class="mx-auto h-12 w-12 bg-[var(--accent)] rounded-xl shadow-sm flex items-center justify-center mb-4">
           <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-7 h-7 text-white"><path d="M12 2.25c-5.385 0-9.75 4.365-9.75 9.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S17.385 2.25 12 2.25zM12.75 9a.75.75 0 00-1.5 0v2.25H9a.75.75 0 000 1.5h2.25V15a.75.75 0 001.5 0v-2.25H15a.75.75 0 000-1.5h-2.25V9z" /></svg>
        </div>
        <h2 class="text-center text-2xl font-bold tracking-tight text-[var(--text)]">Sign in to your account</h2>
        <p class="mt-2 text-center text-sm text-[var(--text-soft)]">Company and company-user onboarding is invite-only.</p>
      </div>

      <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-[400px]">
        <div class="sb-surface py-8 px-4 shadow-sm sm:rounded-xl sm:px-10 border border-[var(--border)]">
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <label class="block">
              <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">Email address</span>
              <input id="email" type="email" formControlName="email" class="sb-input" placeholder="admin@survey-basket.com" />
            </label>

            <label class="block">
              <div class="flex justify-between items-center mb-1.5">
                <span class="block text-sm font-semibold text-[var(--text)]">Password</span>
                <a href="#" class="text-xs font-medium text-[var(--accent)] hover:text-blue-800 transition-colors hidden sm:block">Forgot password?</a>
              </div>
              <input id="password" type="password" formControlName="password" class="sb-input" placeholder="••••••••" />
            </label>

            <div class="pt-2">
              <button type="submit" [disabled]="loginForm.invalid || isLoading" class="sb-btn-primary w-full py-2 shadow-sm font-semibold text-sm">
                {{ isLoading ? 'Signing in...' : 'Sign in' }}
              </button>
            </div>
          </form>

        </div>
        
        <p class="text-center text-xs text-[var(--text-soft)] mt-6">
          Survey Basket Platform &copy; 2024
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  private authService = inject(AuthService);
  private router = inject(Router);
  private uiFeedback = inject(UiFeedbackService);

  constructor(private fb: FormBuilder) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.uiFeedback.success('Welcome back', 'Signed in successfully.');
          this.router.navigate(['/dashboard']);
        },
        error: () => {
          this.isLoading = false;
          this.uiFeedback.error('Login failed', 'Please check your email and password.');
        }
      });
    }
  }

}
