import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../core/services/account.service';
import { CreateCompanyAccountRequest, CreateCompanyAccountResponse } from '../../core/models/company-account';

@Component({
  selector: 'app-companies',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './companies.component.html',
  styleUrls: ['./companies.component.scss']
})
export class CompaniesComponent {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);

  readonly loading = signal(false);
  readonly generatingToken = signal(false);
  readonly created = signal<CreateCompanyAccountResponse | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly form = this.fb.group({
    companyName: ['', [Validators.required, Validators.minLength(2)]],
    contactEmail: ['', [Validators.required, Validators.email]],
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]]
  });

  createCompanyAccount(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.created.set(null);

    this.accountService.createCompanyAccount(this.form.getRawValue() as CreateCompanyAccountRequest).subscribe({
      next: (response) => {
        this.created.set(response);
        this.form.reset();
        this.successMessage.set('Company account created. Share the activation link and token securely.');
        this.loading.set(false);
      },
      error: (error) => {
        if (error?.status === 403) {
          this.errorMessage.set('You are not authorized to create company accounts.');
        } else {
          this.errorMessage.set(error?.error?.detail || 'Company account creation failed. Please retry.');
        }
        this.loading.set(false);
      }
    });
  }

  getActivationPath(companyAccountUserId: string): string {
    return `/activate-company/${companyAccountUserId}`;
  }

  getActivationUrl(companyAccountUserId: string): string {
    if (typeof window === 'undefined') {
      return this.getActivationPath(companyAccountUserId);
    }

    return `${window.location.origin}${this.getActivationPath(companyAccountUserId)}`;
  }

  openActivationPage(companyAccountUserId: string): void {
    if (typeof window !== 'undefined') {
      window.open(this.getActivationUrl(companyAccountUserId), '_blank', 'noopener,noreferrer');
    }
  }

  copyActivationToken(): void {
    const token = this.created()?.activationToken;
    if (!token) {
      return;
    }

    this.copyText(token, 'Activation token copied.');
  }

  copyActivationLink(): void {
    const companyAccountUserId = this.created()?.companyAccountUserId;
    if (!companyAccountUserId) {
      return;
    }

    this.copyText(this.getActivationUrl(companyAccountUserId), 'Activation link copied.');
  }

  regenerateToken(): void {
    const companyAccountUserId = this.created()?.companyAccountUserId;
    if (!companyAccountUserId) {
      return;
    }

    this.generatingToken.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.accountService.generateCompanyActivationToken(companyAccountUserId).subscribe({
      next: (result) => {
        const current = this.created();
        if (!current) {
          this.generatingToken.set(false);
          return;
        }

        this.created.set({
          ...current,
          activationToken: result.activationToken
        });
        this.successMessage.set('A new activation token was generated.');
        this.generatingToken.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error?.error?.detail || 'Failed to regenerate activation token.');
        this.generatingToken.set(false);
      }
    });
  }

  private copyText(value: string, successMessage: string): void {
    if (typeof navigator !== 'undefined' && navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(value)
        .then(() => this.successMessage.set(successMessage))
        .catch(() => this.errorMessage.set('Clipboard copy failed. Please copy manually.'));
      return;
    }

    this.errorMessage.set('Clipboard API is unavailable. Please copy manually.');
  }
}
