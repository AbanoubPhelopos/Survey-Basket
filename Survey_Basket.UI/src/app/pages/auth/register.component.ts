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
    <section class="auth-shell">
      <main class="auth-panel">
        <div class="auth-card sb-surface">
          <h2 class="text-3xl font-bold">Create account</h2>
          <p class="sb-muted">Register to access survey operations.</p>

          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="auth-form">
            <div class="auth-grid-2">
              <label>
                <span>First Name</span>
                <input id="firstName" formControlName="firstName" type="text" class="input-fancy" placeholder="John" />
              </label>
              <label>
                <span>Last Name</span>
                <input id="lastName" formControlName="lastName" type="text" class="input-fancy" placeholder="Doe" />
              </label>
            </div>

            <label>
              <span>Email address</span>
              <input id="email" formControlName="email" type="email" class="input-fancy" placeholder="john@example.com" />
            </label>

            <label>
              <span>Password</span>
              <input id="password" formControlName="password" type="password" class="input-fancy" placeholder="••••••••" />
            </label>

            <button type="submit" [disabled]="registerForm.invalid || isLoading" class="btn-primary">
              {{ isLoading ? 'Creating account...' : 'Create Account' }}
            </button>

            <div class="text-sm">
              <a routerLink="/login" class="font-semibold">Already have an account? Log in</a>
            </div>
          </form>
        </div>
      </main>

      <aside class="auth-hero">
        <h2 class="text-4xl font-bold">Build Better Surveys</h2>
        <p class="text-lg">Provision companies, manage records, and collect data through a polished workflow.</p>
      </aside>
    </section>
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
