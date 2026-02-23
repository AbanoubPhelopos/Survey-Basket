import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { PollService } from '../../core/services/poll.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

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
export class CreatePollComponent {
  pollForm: FormGroup;
  isLoading = false;
  private pollService = inject(PollService);
  private router = inject(Router);
  private uiFeedback = inject(UiFeedbackService);

  constructor(private fb: FormBuilder) {
    const today = new Date().toISOString().split('T')[0];

    this.pollForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      summary: ['', [Validators.required, Validators.maxLength(500)]],
      startedAt: [today, Validators.required],
      endedAt: [''],
      isPublished: [false]
    });
  }

  onSubmit() {
    if (this.pollForm.valid) {
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
