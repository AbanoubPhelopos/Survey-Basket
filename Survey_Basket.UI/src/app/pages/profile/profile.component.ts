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
    <div class="min-h-screen bg-gray-50 py-10 px-4 sm:px-6 lg:px-8">
      <div class="max-w-3xl mx-auto">
        <h1 class="text-2xl font-bold text-gray-900 mb-8">Account Settings</h1>

        <div class="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div class="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h3 class="text-lg leading-6 font-medium text-gray-900">Profile Information</h3>
            <p class="mt-1 max-w-2xl text-sm text-gray-500">Update your personal details.</p>
          </div>
          <div class="px-4 py-5 sm:p-6">
            <form [formGroup]="profileForm" (ngSubmit)="onUpdateProfile()">
              <div class="grid grid-cols-6 gap-6">
                <div class="col-span-6 sm:col-span-3">
                  <label for="first-name" class="block text-sm font-medium text-gray-700">First name</label>
                  <input type="text" formControlName="firstName" id="first-name" class="input-fancy mt-1">
                </div>

                <div class="col-span-6 sm:col-span-3">
                  <label for="last-name" class="block text-sm font-medium text-gray-700">Last name</label>
                  <input type="text" formControlName="lastName" id="last-name" class="input-fancy mt-1">
                </div>

                <div class="col-span-6 sm:col-span-4">
                  <label for="email" class="block text-sm font-medium text-gray-700">Email address</label>
                  <input type="text" [value]="email()" disabled id="email" class="input-fancy mt-1 bg-gray-100 cursor-not-allowed">
                </div>
              </div>
              <div class="mt-6 flex justify-end">
                <button type="submit" [disabled]="profileForm.invalid || isUpdatingProfile" class="btn-primary w-auto px-4 py-2">
                  {{ isUpdatingProfile ? 'Saving...' : 'Save' }}
                </button>
              </div>
            </form>
          </div>
        </div>

        <div class="bg-white shadow overflow-hidden sm:rounded-lg">
          <div class="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h3 class="text-lg leading-6 font-medium text-gray-900">Change Password</h3>
            <p class="mt-1 max-w-2xl text-sm text-gray-500">Ensure your account is using a long, random password to stay secure.</p>
          </div>
          <div class="px-4 py-5 sm:p-6">
            <form [formGroup]="passwordForm" (ngSubmit)="onChangePassword()">
              <div class="grid grid-cols-6 gap-6">
                <div class="col-span-6 sm:col-span-4">
                  <label for="current-password" class="block text-sm font-medium text-gray-700">Current Password</label>
                  <input type="password" formControlName="currentPassword" id="current-password" class="input-fancy mt-1">
                </div>

                <div class="col-span-6 sm:col-span-4">
                  <label for="new-password" class="block text-sm font-medium text-gray-700">New Password</label>
                  <input type="password" formControlName="newPassword" id="new-password" class="input-fancy mt-1">
                </div>
              </div>
              <div class="mt-6 flex justify-end">
                <button type="submit" [disabled]="passwordForm.invalid || isChangingPassword" class="btn-primary w-auto px-4 py-2 bg-gray-800 hover:bg-gray-900">
                  {{ isChangingPassword ? 'Changing...' : 'Change Password' }}
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
