import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import { CreateCompanyUserRecordRequest, CreateCompanyUserRecordResponse } from '../../core/models/company-user-record';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-company-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './company-users.component.html',
  styleUrls: ['./company-users.component.scss']
})
export class CompanyUsersComponent {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly uiFeedback = inject(UiFeedbackService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly records = signal<CreateCompanyUserRecordResponse[]>([]);

  readonly form = this.fb.group({
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    businessIdentifier: ['', [Validators.required, Validators.minLength(3)]]
  });

  createRecord(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.userService.createCompanyUserRecord(this.form.getRawValue() as CreateCompanyUserRecordRequest).subscribe({
      next: (response) => {
        this.records.update((prev) => [response, ...prev]);
        this.form.reset();
        this.uiFeedback.success('Record created', 'Company user record was created successfully.');
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        if (error?.status === 403) {
          this.errorMessage.set('Only company account can create records for its own company.');
        } else {
          this.errorMessage.set(error?.error?.detail || 'Failed to create company user record.');
        }
        this.uiFeedback.error('Create failed', this.errorMessage() || 'Failed to create company user record.');
      }
    });
  }
}
