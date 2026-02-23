import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen bg-[var(--bg)] flex flex-col justify-center py-12 sm:px-6 lg:px-8 font-sans">
      <div class="sm:mx-auto sm:w-full sm:max-w-md text-center">
        <div class="mx-auto h-12 w-12 bg-[var(--accent)] rounded-xl shadow-sm flex items-center justify-center mb-4">
           <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="w-7 h-7 text-white"><path d="M12 2.25c-5.385 0-9.75 4.365-9.75 9.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S17.385 2.25 12 2.25zM12.75 9a.75.75 0 00-1.5 0v2.25H9a.75.75 0 000 1.5h2.25V15a.75.75 0 001.5 0v-2.25H15a.75.75 0 000-1.5h-2.25V9z" /></svg>
        </div>
        <h2 class="text-center text-2xl font-bold tracking-tight text-[var(--text)]">Create your account</h2>
        <p class="mt-2 text-center text-sm text-[var(--text-soft)]">
          Already have an account?
          <a routerLink="/login" class="font-semibold text-[var(--accent)] hover:text-blue-800 transition-colors">Sign in here</a>
        </p>
      </div>

      <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-[440px]">
        <div class="sb-surface py-8 px-4 shadow-sm sm:rounded-xl sm:px-8 border border-[var(--border)]">
          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <div class="grid grid-cols-2 gap-4">
              <label class="block">
                <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">First Name</span>
                <input id="firstName" formControlName="firstName" type="text" class="sb-input" placeholder="John" />
              </label>
              <label class="block">
                <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">Last Name</span>
                <input id="lastName" formControlName="lastName" type="text" class="sb-input" placeholder="Doe" />
              </label>
            </div>

            <label class="block">
              <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">Email address</span>
              <input id="email" formControlName="email" type="email" class="sb-input" placeholder="john@example.com" />
            </label>

            <label class="block">
              <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">Password</span>
              <input id="password" formControlName="password" type="password" class="sb-input" placeholder="••••••••" />
              <p class="text-[0.7rem] text-[var(--text-soft)] mt-1.5 ml-1">Must be at least 6 characters long</p>
            </label>

            <div class="pt-2">
              <button type="submit" [disabled]="registerForm.invalid || isLoading" class="sb-btn-primary w-full py-2 shadow-sm font-semibold text-sm">
                {{ isLoading ? 'Creating account...' : 'Create Account' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  private authService = inject(AuthService);
  private router = inject(Router);
  private uiFeedback = inject(UiFeedbackService);

  constructor(private fb: FormBuilder) {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          this.isLoading = false;
          this.uiFeedback.success('Account created', 'Your account was created. You can now sign in.');
          this.router.navigate(['/login']);
        },
        error: () => {
          this.isLoading = false;
          this.uiFeedback.error('Registration failed', 'Unable to create account. Please try again.');
        }
      });
    }
  }
}
