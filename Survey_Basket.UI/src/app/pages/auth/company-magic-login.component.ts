import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-company-magic-login',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen flex items-center justify-center p-4">
      <div class="sb-surface p-6 rounded-xl max-w-md w-full text-center">
        <h1 class="text-lg font-bold">Company One-Time Login</h1>
        <p class="sb-muted text-sm mt-2">{{ message() }}</p>
      </div>
    </div>
  `
})
export class CompanyMagicLoginComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);
  private readonly ui = inject(UiFeedbackService);
  private readonly router = inject(Router);

  readonly message = signal('Validating your secure link...');

  constructor() {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.message.set('Invalid link.');
      return;
    }

    this.authService.redeemCompanyMagicLink({ token }).subscribe({
      next: () => {
        this.ui.success('Signed in', 'Company account signed in successfully.');
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.message.set('Link is invalid or expired. Request a new magic link.');
      }
    });
  }
}
