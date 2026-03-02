import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-join-company-poll',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen bg-[var(--bg)] flex items-center justify-center p-4">
      <form class="sb-surface p-6 rounded-xl w-full max-w-lg space-y-4" [formGroup]="form" (ngSubmit)="redeem()">
        <h1 class="text-lg font-bold">Join Poll</h1>
        <p class="text-sm sb-muted">Complete your identity details to access this company poll.</p>

        <div class="grid grid-cols-2 gap-3">
          <label class="block"><span class="text-sm font-semibold">First Name</span><input class="sb-input" formControlName="firstName" /></label>
          <label class="block"><span class="text-sm font-semibold">Last Name</span><input class="sb-input" formControlName="lastName" /></label>
        </div>

        <label class="block"><span class="text-sm font-semibold">Email</span><input class="sb-input" formControlName="email" type="email" /></label>
        <label class="block"><span class="text-sm font-semibold">Mobile</span><input class="sb-input" formControlName="mobile" placeholder="e.g. +201234567890" /></label>
        <label class="block"><span class="text-sm font-semibold">Password for future logins</span><input type="password" class="sb-input" formControlName="password" /></label>

        @if (errorMessage) {
          <p class="text-sm text-red-700">{{ errorMessage }}</p>
        }

        <button class="sb-btn-primary w-full" type="submit" [disabled]="form.invalid || loading">{{ loading ? 'Joining...' : 'Join Poll' }}</button>
      </form>
    </div>
  `
})
export class JoinCompanyPollComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly ui = inject(UiFeedbackService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  loading = false;
  errorMessage: string | null = null;

  form = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    mobile: ['', [Validators.required, Validators.minLength(7)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  redeem(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.errorMessage = 'Invite token is missing.';
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    const value = this.form.getRawValue();

    this.auth.redeemCompanyPollAccess({
      token,
      firstName: value.firstName || '',
      lastName: value.lastName || '',
      email: value.email || '',
      mobile: value.mobile || '',
      password: value.password || ''
    }).subscribe({
      next: (response) => {
        this.ui.success('Access granted', 'You can now submit your poll response.');
        if (response.redirectPollId) {
          this.router.navigate(['/polls', response.redirectPollId, 'vote']);
          return;
        }

        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error?.error?.detail || 'Access link is invalid, expired, or already used.';
      }
    });
  }
}
