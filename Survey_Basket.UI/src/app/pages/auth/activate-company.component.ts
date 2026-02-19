import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ActivateCompanyAccountRequest } from '../../core/models/auth';

@Component({
  selector: 'app-activate-company',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './activate-company.component.html',
  styleUrls: ['./activate-company.component.scss']
})
export class ActivateCompanyComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.group({
    activationToken: ['', [Validators.required, Validators.minLength(10)]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]]
  });

  activate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const companyAccountUserId = this.route.snapshot.paramMap.get('companyAccountUserId');
    if (!companyAccountUserId) {
      this.errorMessage.set('Missing company account user id in activation link.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.activateCompanyAccount(companyAccountUserId, this.form.getRawValue() as ActivateCompanyAccountRequest).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.loading.set(false);
        this.errorMessage.set(error?.error?.detail || 'Activation failed. Please verify token and try again.');
      }
    });
  }
}
