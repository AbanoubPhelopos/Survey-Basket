import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-company-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen bg-[var(--bg)] flex items-center justify-center p-4">
      <form class="sb-surface p-6 rounded-xl w-full max-w-md space-y-4" [formGroup]="form" (ngSubmit)="submit()">
        <h1 class="text-lg font-bold text-[var(--text)]">Set your company password</h1>
        <p class="text-sm text-[var(--text-soft)]">
          For security, you must create a new password before continuing.
        </p>

        <label class="block">
          <span class="text-sm font-semibold text-[var(--text)]">New password</span>
          <input type="password" class="sb-input mt-1" formControlName="newPassword" placeholder="At least 6 characters" />
        </label>

        <label class="block">
          <span class="text-sm font-semibold text-[var(--text)]">Confirm password</span>
          <input type="password" class="sb-input mt-1" formControlName="confirmPassword" placeholder="Re-enter password" />
        </label>

        @if (errorMessage()) {
          <p class="text-sm text-red-700">{{ errorMessage() }}</p>
        }

        <button class="sb-btn-primary w-full" type="submit" [disabled]="saving() || form.invalid">
          {{ saving() ? 'Saving...' : 'Set Password' }}
        </button>

        <button class="sb-btn-secondary w-full" type="button" (click)="logout()" [disabled]="saving()">
          Sign out
        </button>
      </form>
    </div>
  `
})
export class CompanyChangePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly authService = inject(AuthService);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isCompanyAccount = computed(() => this.authService.hasRole('PartnerCompany'));

  protected readonly form = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
  });

  protected submit(): void {
    this.errorMessage.set(null);

    if (!this.isCompanyAccount()) {
      this.router.navigate(['/dashboard']);
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const newPassword = this.form.controls.newPassword.value ?? '';
    const confirmPassword = this.form.controls.confirmPassword.value ?? '';

    if (newPassword !== confirmPassword) {
      this.errorMessage.set('New password and confirmation do not match.');
      return;
    }

    this.saving.set(true);
    this.accountService.setInitialPassword(newPassword).subscribe({
      next: () => {
        this.saving.set(false);
        this.authService.markPasswordSetupCompleted();
        this.uiFeedback.success('Password updated', 'Please sign in again with your new password.');
        this.authService.logout();
      },
      error: (error: { error?: { detail?: string } }) => {
        this.saving.set(false);
        this.errorMessage.set(error?.error?.detail || 'Unable to set password. Please try again.');
      }
    });
  }

  protected logout(): void {
    this.authService.logout();
  }
}
