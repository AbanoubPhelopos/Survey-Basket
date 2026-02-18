import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen flex bg-gray-50">
      <!-- Right: Register Form (Swapped for variety) -->
      <div class="w-full lg:w-1/2 flex items-center justify-center p-8 sm:p-12 lg:p-16 bg-white shadow-xl lg:shadow-none z-10">
        <div class="w-full max-w-md space-y-8">
          <div class="text-center lg:text-left">
            <h2 class="text-3xl font-bold text-gray-900 tracking-tight">Create your account</h2>
            <p class="mt-2 text-sm text-gray-600">
              Start building better surveys today. No credit card required.
            </p>
          </div>

          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="mt-8 space-y-6">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label for="firstName" class="block text-sm font-medium text-gray-700 mb-1">First Name</label>
                <input id="firstName" formControlName="firstName" type="text" class="input-fancy" placeholder="John">
                <p *ngIf="registerForm.get('firstName')?.touched && registerForm.get('firstName')?.invalid" class="mt-1 text-xs text-red-500">Required.</p>
              </div>
              <div>
                <label for="lastName" class="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
                <input id="lastName" formControlName="lastName" type="text" class="input-fancy" placeholder="Doe">
                <p *ngIf="registerForm.get('lastName')?.touched && registerForm.get('lastName')?.invalid" class="mt-1 text-xs text-red-500">Required.</p>
              </div>
            </div>

            <div>
              <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email address</label>
              <input id="email" formControlName="email" type="email" class="input-fancy" placeholder="john@example.com">
              <p *ngIf="registerForm.get('email')?.touched && registerForm.get('email')?.invalid" class="mt-1 text-xs text-red-500">Valid email required.</p>
            </div>

            <div>
              <label for="password" class="block text-sm font-medium text-gray-700 mb-1">Password</label>
              <input id="password" formControlName="password" type="password" class="input-fancy" placeholder="••••••••">
              <p *ngIf="registerForm.get('password')?.touched && registerForm.get('password')?.invalid" class="mt-1 text-xs text-red-500">Min 6 chars required.</p>
            </div>

            <div class="flex items-start">
              <div class="flex items-center h-5">
                <input id="terms" type="checkbox" class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded">
              </div>
              <div class="ml-3 text-sm">
                <label for="terms" class="font-medium text-gray-700">I agree to the <a href="#" class="text-primary-600 hover:text-primary-500">Terms</a> and <a href="#" class="text-primary-600 hover:text-primary-500">Privacy Policy</a></label>
              </div>
            </div>

            <button type="submit" [disabled]="registerForm.invalid || isLoading" class="btn-primary flex justify-center items-center gap-2">
              <span *ngIf="isLoading" class="animate-spin h-5 w-5 border-2 border-white border-t-transparent rounded-full"></span>
              {{ isLoading ? 'Creating account...' : 'Create Account' }}
            </button>

            <div class="mt-6 text-center text-sm text-gray-600">
              Already have an account? 
              <a routerLink="/login" class="font-semibold text-primary-600 hover:text-primary-500 transition-colors">Log in</a>
            </div>
          </form>
        </div>
      </div>

      <!-- Left: Decorative Image/Gradient (Swapped) -->
      <div class="hidden lg:flex w-1/2 bg-gradient-to-bl from-indigo-900 to-purple-800 items-center justify-center relative overflow-hidden">
        <div class="absolute inset-0 bg-black/20"></div>
        <div class="relative z-10 text-center text-white px-12">
          <h2 class="text-4xl font-bold mb-6 tracking-tight">Join the Community</h2>
          <p class="text-lg opacity-90 leading-relaxed max-w-md mx-auto">
            "The best way to predict the future is to create it." <br>
            Start gathering insights today.
          </p>
        </div>
        <!-- Decorative blobs -->
        <div class="absolute top-1/4 left-1/4 w-72 h-72 bg-purple-500/30 rounded-full blur-3xl animate-pulse"></div>
        <div class="absolute bottom-1/4 right-1/4 w-96 h-96 bg-blue-500/20 rounded-full blur-3xl"></div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  private authService = inject(AuthService);
  private router = inject(Router);

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
          // Optionally show success message
          this.router.navigate(['/login']);
        },
        error: (err) => {
          console.error(err);
          this.isLoading = false;
          alert('Registration failed. Please try again.');
        }
      });
    }
  }
}
