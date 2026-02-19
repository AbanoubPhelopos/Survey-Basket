import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <section class="auth-shell">
      <aside class="auth-hero">
        <h1 class="text-5xl font-bold">Survey Basket</h1>
        <p class="text-xl">Create and analyze surveys with speed, clarity, and confidence.</p>
      </aside>

      <main class="auth-panel">
        <div class="auth-card sb-surface">
          <h2 class="text-3xl font-bold">Welcome back</h2>
          <p class="sb-muted">Sign in with your admin or company account.</p>

          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="auth-form">
            <label>
              <span>Email address</span>
              <input id="email" type="email" formControlName="email" class="input-fancy" placeholder="admin@survey-basket.com" />
            </label>

            <label>
              <span>Password</span>
              <input id="password" type="password" formControlName="password" class="input-fancy" placeholder="••••••••" />
            </label>

            <button type="submit" [disabled]="loginForm.invalid || isLoading" class="btn-primary">
              {{ isLoading ? 'Signing in...' : 'Sign in' }}
            </button>

            <div class="text-sm">
              <a routerLink="/register" class="font-semibold">Need an account? Register</a>
            </div>
          </form>
        </div>
      </main>
    </section>
  `
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  private authService = inject(AuthService);
  private router = inject(Router);

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
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.error(err);
          this.isLoading = false;
          // Ideally show a toast notification here
          alert('Login failed. Please check your credentials.');
        }
      });
    }
  }
}
