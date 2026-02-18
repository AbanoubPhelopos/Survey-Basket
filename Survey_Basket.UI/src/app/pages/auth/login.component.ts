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
    <div class="min-h-screen flex bg-gray-50">
      <!-- Left: Decorative Image/Gradient -->
      <div class="hidden lg:flex w-1/2 bg-gradient-to-br from-primary-600 to-indigo-800 items-center justify-center relative overflow-hidden">
        <div class="absolute inset-0 bg-black/10"></div>
        <div class="relative z-10 text-center text-white px-12">
          <h1 class="text-5xl font-bold mb-6 tracking-tight">Survey Basket</h1>
          <p class="text-xl opacity-90 leading-relaxed">
            Create, manage, and analyze surveys effortlessly. <br>
            Empower your decisions with real data.
          </p>
        </div>
        <!-- Abstract decorative circles -->
        <div class="absolute top-0 left-0 w-96 h-96 bg-white/10 rounded-full blur-3xl -translate-x-1/2 -translate-y-1/2"></div>
        <div class="absolute bottom-0 right-0 w-96 h-96 bg-indigo-500/20 rounded-full blur-3xl translate-x-1/2 translate-y-1/2"></div>
      </div>

      <!-- Right: Login Form -->
      <div class="w-full lg:w-1/2 flex items-center justify-center p-8 sm:p-12 lg:p-16">
        <div class="w-full max-w-md space-y-8">
          <div class="text-center lg:text-left">
            <h2 class="text-3xl font-bold text-gray-900 tracking-tight">Welcome back</h2>
            <p class="mt-2 text-sm text-gray-600">
              Please enter your details to sign in.
            </p>
          </div>

          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="mt-8 space-y-6">
            <div class="space-y-4">
              <div>
                <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email address</label>
                <input id="email" type="email" formControlName="email" class="input-fancy" placeholder="Enter your email">
                <p *ngIf="loginForm.get('email')?.touched && loginForm.get('email')?.invalid" class="mt-1 text-xs text-red-500">
                  Please enter a valid email address.
                </p>
              </div>

              <div>
                <label for="password" class="block text-sm font-medium text-gray-700 mb-1">Password</label>
                <input id="password" type="password" formControlName="password" class="input-fancy" placeholder="••••••••">
                <p *ngIf="loginForm.get('password')?.touched && loginForm.get('password')?.invalid" class="mt-1 text-xs text-red-500">
                  Password is required.
                </p>
              </div>
            </div>

            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <input id="remember-me" type="checkbox" class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded">
                <label for="remember-me" class="ml-2 block text-sm text-gray-600">Remember me</label>
              </div>
              <div class="text-sm">
                <a href="#" class="font-medium text-primary-600 hover:text-primary-500 transition-colors">Forgot password?</a>
              </div>
            </div>

            <button type="submit" [disabled]="loginForm.invalid || isLoading" class="btn-primary flex justify-center items-center gap-2">
              <span *ngIf="isLoading" class="animate-spin h-5 w-5 border-2 border-white border-t-transparent rounded-full"></span>
              {{ isLoading ? 'Signing in...' : 'Sign in' }}
            </button>

            <div class="mt-6 text-center text-sm text-gray-600">
              Don't have an account? 
              <a routerLink="/register" class="font-semibold text-primary-600 hover:text-primary-500 transition-colors">Sign up for free</a>
            </div>
          </form>
        </div>
      </div>
    </div>
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
