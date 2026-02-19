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
  readonly created = signal<CreateCompanyAccountResponse | null>(null);
  readonly errorMessage = signal<string | null>(null);

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
    this.created.set(null);

    this.accountService.createCompanyAccount(this.form.getRawValue() as CreateCompanyAccountRequest).subscribe({
      next: (response) => {
        this.created.set(response);
        this.form.reset();
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
}
