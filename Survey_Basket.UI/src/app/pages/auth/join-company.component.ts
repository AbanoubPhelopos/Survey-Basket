import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-join-company',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen bg-[var(--bg)] flex items-center justify-center p-4">
      <form class="sb-surface p-6 rounded-xl w-full max-w-lg space-y-4" [formGroup]="form" (ngSubmit)="redeem()">
        <h1 class="text-lg font-bold">Join Your Company</h1>
        <p class="text-sm sb-muted">Scan QR or open your secure invite link, then complete onboarding.</p>

        <div class="grid grid-cols-2 gap-3">
          <label class="block"><span class="text-sm font-semibold">First Name</span><input class="sb-input" formControlName="firstName" /></label>
          <label class="block"><span class="text-sm font-semibold">Last Name</span><input class="sb-input" formControlName="lastName" /></label>
        </div>

        <label class="block"><span class="text-sm font-semibold">Email (optional)</span><input class="sb-input" formControlName="email" /></label>
        <label class="block"><span class="text-sm font-semibold">Mobile (optional)</span><input class="sb-input" formControlName="mobile" /></label>
        <label class="block"><span class="text-sm font-semibold">Password for future logins (optional)</span><input type="password" class="sb-input" formControlName="password" /></label>

        <button class="sb-btn-primary w-full" type="submit" [disabled]="form.invalid || loading">{{ loading ? 'Joining...' : 'Join Company' }}</button>
      </form>
    </div>
  `
})
export class JoinCompanyComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly ui = inject(UiFeedbackService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  loading = false;

  form = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: [''],
    mobile: [''],
    password: ['']
  });

  redeem(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.ui.error('Invalid link', 'Invite token is missing.');
      return;
    }

    const value = this.form.getRawValue();
    if (!value.email && !value.mobile) {
      this.ui.warning('Identity required', 'Provide email or mobile.');
      return;
    }

    this.loading = true;
    this.auth.redeemCompanyUserInvite({
      token,
      firstName: value.firstName || '',
      lastName: value.lastName || '',
      email: value.email || undefined,
      mobile: value.mobile || undefined,
      password: value.password || undefined
    }).subscribe({
      next: () => {
        this.ui.success('Joined', 'Your company account is ready. Complete your profile first.');
        this.router.navigate(['/profile']);
      },
      error: () => {
        this.loading = false;
        this.ui.error('Join failed', 'Invite invalid, expired, or already used.');
      }
    });
  }
}
