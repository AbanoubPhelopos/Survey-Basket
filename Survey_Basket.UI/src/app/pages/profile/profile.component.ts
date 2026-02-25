import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserProfileResponse } from '../../core/models/account';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-wrapper py-6">
      <header class="mb-6">
        <h1 class="text-2xl font-bold text-[var(--text)]">Profile</h1>
        <p class="text-sm text-[var(--text-soft)] mt-1">Update your account details and security settings.</p>
      </header>

      @if (loadError()) {
        <div class="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{{ loadError() }}</div>
      }

      <div class="grid gap-5">
        <article class="sb-surface border border-[var(--border)] rounded-xl p-5">
          <div class="flex items-center justify-between gap-4 mb-4">
            <h2 class="text-lg font-semibold text-[var(--text)]">Personal information</h2>
            @if (requiresProfileCompletion()) {
              <span class="inline-flex items-center rounded px-2 py-1 text-[0.65rem] font-bold uppercase tracking-wider bg-amber-50 text-amber-700 border border-amber-200">Required</span>
            }
          </div>

          <form [formGroup]="profileForm" (ngSubmit)="saveProfile()" class="grid gap-4 md:grid-cols-2">
            <label class="block">
              <span class="text-sm font-semibold text-[var(--text)]">First name</span>
              <input type="text" formControlName="firstName" class="sb-input mt-1" placeholder="First name" />
            </label>
            <label class="block">
              <span class="text-sm font-semibold text-[var(--text)]">Last name</span>
              <input type="text" formControlName="lastName" class="sb-input mt-1" placeholder="Last name" />
            </label>
            <label class="block md:col-span-2">
              <span class="text-sm font-semibold text-[var(--text)]">Email address</span>
              <input type="email" formControlName="email" class="sb-input mt-1" readonly />
            </label>

            <div class="md:col-span-2 pt-1">
              <button type="submit" class="sb-btn-primary" [disabled]="savingProfile() || profileForm.invalid">
                {{ savingProfile() ? 'Saving...' : 'Save profile' }}
              </button>
            </div>
          </form>
        </article>

        <article class="sb-surface border border-[var(--border)] rounded-xl p-5">
          <div class="flex items-center justify-between gap-4 mb-4">
            <h2 class="text-lg font-semibold text-[var(--text)]">Password</h2>
            @if (requiresPasswordSetup()) {
              <span class="inline-flex items-center rounded px-2 py-1 text-[0.65rem] font-bold uppercase tracking-wider bg-amber-50 text-amber-700 border border-amber-200">Setup required</span>
            }
          </div>

          <form [formGroup]="passwordForm" (ngSubmit)="savePassword()" class="grid gap-4 md:grid-cols-2">
            @if (!requiresPasswordSetup()) {
              <label class="block md:col-span-2">
                <span class="text-sm font-semibold text-[var(--text)]">Current password</span>
                <input type="password" formControlName="currentPassword" class="sb-input mt-1" placeholder="Current password" />
              </label>
            }

            <label class="block">
              <span class="text-sm font-semibold text-[var(--text)]">New password</span>
              <input type="password" formControlName="newPassword" class="sb-input mt-1" placeholder="At least 6 characters" />
            </label>
            <label class="block">
              <span class="text-sm font-semibold text-[var(--text)]">Confirm password</span>
              <input type="password" formControlName="confirmPassword" class="sb-input mt-1" placeholder="Re-enter password" />
            </label>

            @if (passwordError()) {
              <p class="md:col-span-2 text-sm text-red-700">{{ passwordError() }}</p>
            }

            <div class="md:col-span-2 pt-1">
              <button type="submit" class="sb-btn-primary" [disabled]="savingPassword() || passwordForm.invalid">
                {{ savingPassword() ? 'Saving...' : (requiresPasswordSetup() ? 'Set password' : 'Change password') }}
              </button>
            </div>
          </form>
        </article>
      </div>
    </section>
  `
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly authService = inject(AuthService);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly router = inject(Router);

  protected readonly user = this.authService.user;
  protected readonly requiresProfileCompletion = computed(() => !!this.user()?.requiresProfileCompletion);
  protected readonly requiresPasswordSetup = computed(() => !!this.user()?.requiresPasswordSetup);
  protected readonly savingProfile = signal(false);
  protected readonly savingPassword = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly passwordError = signal<string | null>(null);

  protected readonly profileForm = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: [{ value: '', disabled: true }]
  });

  protected readonly passwordForm = this.fb.group({
    currentPassword: [''],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    const currentUser = this.user();
    if (currentUser) {
      this.profileForm.patchValue({
        firstName: currentUser.firstName ?? '',
        lastName: currentUser.lastName ?? '',
        email: currentUser.email ?? ''
      });
    }

    this.accountService.getProfile().subscribe({
      next: (profile: UserProfileResponse) => {
        this.profileForm.patchValue({
          firstName: profile.firstName,
          lastName: profile.lastName,
          email: profile.email
        });
      },
      error: () => {
        this.loadError.set('Unable to load the latest profile details. You can still update your information.');
      }
    });
  }

  protected saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const firstName = this.profileForm.controls.firstName.value?.trim() ?? '';
    const lastName = this.profileForm.controls.lastName.value?.trim() ?? '';

    this.savingProfile.set(true);
    this.accountService.updateProfile({ firstName, lastName }).subscribe({
      next: () => {
        this.savingProfile.set(false);
        this.authService.markProfileCompleted();
        this.uiFeedback.success('Profile updated', 'Your personal details were saved.');
        this.navigateIfRequirementsCleared();
      },
      error: (error: { error?: { detail?: string } }) => {
        this.savingProfile.set(false);
        this.uiFeedback.error('Unable to save profile', error?.error?.detail || 'Please try again.');
      }
    });
  }

  protected savePassword(): void {
    this.passwordError.set(null);

    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const newPassword = this.passwordForm.controls.newPassword.value ?? '';
    const confirmPassword = this.passwordForm.controls.confirmPassword.value ?? '';
    const currentPassword = this.passwordForm.controls.currentPassword.value ?? '';

    if (newPassword !== confirmPassword) {
      this.passwordError.set('New password and confirmation do not match.');
      return;
    }

    this.savingPassword.set(true);

    if (this.requiresPasswordSetup()) {
      this.accountService.setInitialPassword(newPassword).subscribe({
        next: () => {
          this.savingPassword.set(false);
          this.authService.markPasswordSetupCompleted();
          this.passwordForm.reset();
          this.uiFeedback.success('Password set', 'Your password has been configured successfully.');
          this.navigateIfRequirementsCleared();
        },
        error: (error: { error?: { detail?: string } }) => {
          this.savingPassword.set(false);
          this.uiFeedback.error('Unable to set password', error?.error?.detail || 'Please try again.');
        }
      });
      return;
    }

    if (!currentPassword) {
      this.savingPassword.set(false);
      this.passwordError.set('Current password is required to change your password.');
      return;
    }

    this.accountService.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordForm.reset();
        this.uiFeedback.success('Password updated', 'Your password has been changed.');
      },
      error: (error: { error?: { detail?: string } }) => {
        this.savingPassword.set(false);
        this.uiFeedback.error('Unable to change password', error?.error?.detail || 'Please try again.');
      }
    });
  }

  private navigateIfRequirementsCleared(): void {
    if (this.requiresProfileCompletion() || this.requiresPasswordSetup()) {
      return;
    }

    const redirectPollId = this.user()?.redirectPollId;
    if (redirectPollId) {
      this.router.navigate(['/polls', redirectPollId, 'vote']);
      return;
    }

    this.router.navigate(['/dashboard']);
  }
}
