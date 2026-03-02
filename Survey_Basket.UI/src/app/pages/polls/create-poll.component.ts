import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { PollService } from '../../core/services/poll.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { RequestFilters } from '../../core/models/poll';

@Component({
  selector: 'app-create-poll',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="page-wrapper pt-4 max-w-3xl mx-auto w-full">
      <div class="flex items-center justify-end gap-4">
        <a routerLink="/dashboard" class="px-3 py-1.5 text-xs font-semibold rounded-md border border-[var(--border)] hover:bg-[var(--sidebar-hover)] transition-colors">Cancel</a>
      </div>

      <form [formGroup]="pollForm" (ngSubmit)="onSubmit()" class="sb-surface p-6 rounded-xl space-y-5">
        <label class="block">
          <span class="block text-sm font-semibold mb-1.5">Title</span>
          <input type="text" id="title" formControlName="title" class="sb-input" placeholder="e.g., Employee Satisfaction Survey 2024" />
        </label>

        <label class="block">
          <span class="block text-sm font-semibold mb-1.5">Summary</span>
          <textarea id="summary" formControlName="summary" rows="4" class="sb-input" placeholder="Brief description..."></textarea>
        </label>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <label class="block">
            <span class="block text-sm font-semibold mb-1.5">Start Date</span>
            <input type="date" id="startedAt" formControlName="startedAt" class="sb-input" />
          </label>
          <label class="block">
            <span class="block text-sm font-semibold mb-1.5">End Date</span>
            <input type="date" id="endedAt" formControlName="endedAt" class="sb-input" />
          </label>
        </div>

        @if (isAdminContext()) {
          <div class="space-y-3">
            <div>
              <h3 class="text-sm font-semibold">Target Companies</h3>
              <p class="text-xs text-[var(--text-soft)] mt-1">Select one or more companies that can view and answer this poll.</p>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 max-h-56 overflow-auto border border-[var(--border)] rounded-lg p-3">
              @for (company of companyOptions(); track company.id) {
                <label class="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    [checked]="isCompanySelected(company.id)"
                    (change)="toggleCompanySelection(company.id, $any($event.target).checked)"
                  />
                  <span>{{ company.name }}</span>
                </label>
              }

              @if (!companyOptions().length) {
                <p class="text-xs text-[var(--text-soft)]">No active companies available.</p>
              }
            </div>

            @if (audienceError()) {
              <p class="text-sm text-red-700">{{ audienceError() }}</p>
            }
          </div>
        }

        <div class="pt-4 mt-6 border-t border-[var(--border)] flex justify-end gap-3">
          <a routerLink="/dashboard" class="px-4 py-2 border border-[var(--border)] rounded-md font-semibold text-sm hover:bg-[var(--sidebar-hover)] transition-colors">Cancel</a>
          <button type="submit" [disabled]="pollForm.invalid || isLoading" class="sb-btn-primary">
            {{ isLoading ? 'Creating...' : 'Create Poll' }}
          </button>
        </div>
      </form>
    </div>
  `
})
export class CreatePollComponent implements OnInit {
  pollForm: FormGroup;
  isLoading = false;
  readonly isAdminContext = signal(false);
  readonly companyOptions = signal<Array<{ id: string; name: string }>>([]);
  readonly audienceError = signal<string | null>(null);

  private readonly companyFilters: RequestFilters = {
    pageNumber: 1,
    pageSize: 200,
    sortColumn: 'CompanyName',
    sortDirection: 'ASC'
  };

  private pollService = inject(PollService);
  private userService = inject(UserService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private uiFeedback = inject(UiFeedbackService);

  constructor(private fb: FormBuilder) {
    const today = new Date().toISOString().split('T')[0];

    this.pollForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      summary: ['', [Validators.required, Validators.maxLength(500)]],
      startedAt: [today, Validators.required],
      endedAt: [''],
      isPublished: [false],
      targetCompanyIds: [[] as string[]]
    });
  }

  ngOnInit(): void {
    this.isAdminContext.set(this.authService.hasAnyRole(['Admin', 'SystemAdmin']));
    if (!this.isAdminContext()) {
      return;
    }

    this.userService.getCompanyAccounts(this.companyFilters, 'active').subscribe({
      next: (result) => {
        const unique = new Map<string, string>();
        result.items.items.forEach((item) => {
          unique.set(item.companyId, item.companyName);
        });

        this.companyOptions.set(Array.from(unique.entries()).map(([id, name]) => ({ id, name })));
      },
      error: () => {
        this.uiFeedback.error('Load failed', 'Unable to load companies for poll targeting.');
      }
    });
  }

  toggleCompanySelection(companyId: string, checked: boolean): void {
    const selected = this.pollForm.controls['targetCompanyIds'].value ?? [];
    if (checked) {
      if (!selected.includes(companyId)) {
        this.pollForm.controls['targetCompanyIds'].setValue([...selected, companyId]);
      }
      this.audienceError.set(null);
      return;
    }

    this.pollForm.controls['targetCompanyIds'].setValue(selected.filter((id: string) => id !== companyId));
  }

  isCompanySelected(companyId: string): boolean {
    const selected = this.pollForm.controls['targetCompanyIds'].value ?? [];
    return selected.includes(companyId);
  }

  onSubmit() {
    if (this.pollForm.valid) {
      const selectedCompanyIds = this.pollForm.controls['targetCompanyIds'].value ?? [];
      if (this.isAdminContext() && selectedCompanyIds.length === 0) {
        this.audienceError.set('Select at least one company to target this poll.');
        return;
      }

      this.isLoading = true;
      this.pollService.createPoll(this.pollForm.value).subscribe({
        next: () => {
          this.uiFeedback.success('Survey created', 'Your new survey is ready for editing and publishing.');
          this.router.navigate(['/dashboard']);
        },
        error: () => {
          this.isLoading = false;
          this.uiFeedback.error('Create failed', 'Unable to create survey right now. Please retry.');
        }
      });
    }
  }
}
