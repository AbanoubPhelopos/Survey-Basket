import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-wrapper pt-4 max-w-4xl mx-auto w-full">
      <header class="page-header flex items-center justify-between gap-4">
        <div>
          <p class="text-xs tracking-wider text-[var(--accent)] font-bold mb-1 uppercase">User Settings</p>
          <h1 class="page-header__title">Account Profile</h1>
          <p class="page-header__desc">Manage your personal information and password security.</p>
        </div>
      </header>

      <div class="grid gap-6 auto-rows-max lg:grid-cols-12">
        <!-- Profile Form -->
        <div class="sb-surface rounded-xl border border-[var(--border)] overflow-hidden lg:col-span-12">
          <div class="px-6 py-5 border-b border-[var(--border)] bg-[var(--bg-soft)]">
            <h3 class="text-[0.95rem] font-bold text-[var(--text)]">Profile Information</h3>
            <p class="mt-1 text-sm text-[var(--text-soft)]">Update your personal details.</p>
          </div>
          <div class="p-6">
            <form [formGroup]="profileForm" (ngSubmit)="onUpdateProfile()">
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <label class="block">
                  <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">First name</span>
                  <input type="text" formControlName="firstName" class="sb-input">
                </label>

                <label class="block">
                  <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">Last name</span>
                  <input type="text" formControlName="lastName" class="sb-input">
                </label>

                <label class="block sm:col-span-2">
                  <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">Email address</span>
                  <input type="text" [value]="email()" disabled class="sb-input cursor-not-allowed text-[var(--text-soft)] bg-[var(--sidebar-hover)]">
                  <p class="text-xs text-[var(--text-soft)] mt-1.5">Email address cannot be changed.</p>
                </label>
              </div>
              <div class="mt-6 flex justify-end">
                <button type="submit" [disabled]="profileForm.invalid || isUpdatingProfile" class="sb-btn-primary shadow-sm px-6">
                  {{ isUpdatingProfile ? 'Saving Changes...' : 'Save Profile' }}
                </button>
              </div>
            </form>
          </div>
        </div>

        <!-- Password Form -->
        <div class="sb-surface rounded-xl border border-[var(--border)] overflow-hidden lg:col-span-12">
          <div class="px-6 py-5 border-b border-[var(--border)] bg-[var(--bg-soft)]">
            <h3 class="text-[0.95rem] font-bold text-[var(--text)]">Change Password</h3>
            <p class="mt-1 text-sm text-[var(--text-soft)]">Ensure your account is using a long, random password to stay secure.</p>
          </div>
          <div class="p-6">
            <form [formGroup]="passwordForm" (ngSubmit)="onChangePassword()">
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <label class="block">
                  <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">Current Password</span>
                  <input type="password" formControlName="currentPassword" class="sb-input">
                </label>

                <label class="block">
                  <span class="block text-sm font-semibold mb-1.5 text-[var(--text)]">New Password</span>
                  <input type="password" formControlName="newPassword" class="sb-input">
                  <p class="text-xs text-[var(--text-soft)] mt-1.5">Minimum 6 characters required.</p>
                </label>
              </div>
              <div class="mt-6 flex justify-end">
                <button type="submit" [disabled]="passwordForm.invalid || isChangingPassword" class="sb-btn-primary shadow-sm px-6">
                  {{ isChangingPassword ? 'Updating Password...' : 'Change Password' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup;
  passwordForm: FormGroup;

  isUpdatingProfile = false;
  isChangingPassword = false;
  email = signal('');

  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private authService = inject(AuthService);

  constructor() {
    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.accountService.getProfile().subscribe(user => {
      this.email.set(user.email);
      this.profileForm.patchValue({
        firstName: user.firstName,
        lastName: user.lastName
      });
    });
  }

  onUpdateProfile() {
    if (this.profileForm.valid) {
      this.isUpdatingProfile = true;
      this.accountService.updateProfile(this.profileForm.value).subscribe({
        next: () => {
          this.isUpdatingProfile = false;
          alert('Profile updated successfully.');
        },
        error: () => {
          this.isUpdatingProfile = false;
          alert('Failed to update profile.');
        }
      });
    }
  }

  onChangePassword() {
    if (this.passwordForm.valid) {
      this.isChangingPassword = true;
      this.accountService.changePassword(this.passwordForm.value).subscribe({
        next: () => {
          this.isChangingPassword = false;
          this.passwordForm.reset();
          alert('Password changed successfully.');
        },
        error: () => {
          this.isChangingPassword = false;
          alert('Failed to change password.');
        }
      });
    }
  }
}
