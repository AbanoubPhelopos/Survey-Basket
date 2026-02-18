import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { PollService } from '../../core/services/poll.service';

@Component({
  selector: 'app-create-poll',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen bg-gray-50 flex flex-col">
      <!-- Simple Header -->
      <header class="bg-white shadow-sm border-b border-gray-200">
        <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <h1 class="text-xl font-bold text-gray-900">Create New Poll</h1>
          <a routerLink="/dashboard" class="text-gray-500 hover:text-gray-700 text-sm font-medium">Cancel</a>
        </div>
      </header>

      <main class="flex-1 py-10 px-4 sm:px-6 lg:px-8">
        <div class="max-w-3xl mx-auto">
          <form [formGroup]="pollForm" (ngSubmit)="onSubmit()" class="space-y-8 bg-white p-8 rounded-xl shadow-sm border border-gray-100">
            
            <!-- Basic Info Section -->
            <div class="space-y-6">
              <div>
                <h3 class="text-lg font-medium leading-6 text-gray-900">Poll Details</h3>
                <p class="mt-1 text-sm text-gray-500">Provide the basic information for your new survey.</p>
              </div>

              <div class="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
                <!-- Title -->
                <div class="sm:col-span-6">
                  <label for="title" class="block text-sm font-medium text-gray-700">Title</label>
                  <div class="mt-1">
                    <input type="text" id="title" formControlName="title" class="input-fancy" placeholder="e.g., Employee Satisfaction Survey 2024">
                  </div>
                  <p *ngIf="pollForm.get('title')?.touched && pollForm.get('title')?.invalid" class="mt-2 text-sm text-red-600">Title is required.</p>
                </div>

                <!-- Summary -->
                <div class="sm:col-span-6">
                  <label for="summary" class="block text-sm font-medium text-gray-700">Summary</label>
                  <div class="mt-1">
                    <textarea id="summary" formControlName="summary" rows="3" class="input-fancy" placeholder="Brief description of what this poll is about..."></textarea>
                  </div>
                  <p *ngIf="pollForm.get('summary')?.touched && pollForm.get('summary')?.invalid" class="mt-2 text-sm text-red-600">Summary is required.</p>
                </div>

                <!-- Dates -->
                <div class="sm:col-span-3">
                  <label for="startedAt" class="block text-sm font-medium text-gray-700">Start Date</label>
                  <div class="mt-1">
                    <input type="date" id="startedAt" formControlName="startedAt" class="input-fancy">
                  </div>
                </div>

                <div class="sm:col-span-3">
                  <label for="endedAt" class="block text-sm font-medium text-gray-700">End Date</label>
                  <div class="mt-1">
                    <input type="date" id="endedAt" formControlName="endedAt" class="input-fancy">
                  </div>
                </div>
              </div>
            </div>

            <div class="pt-5 border-t border-gray-200">
              <div class="flex justify-end gap-3">
                <a routerLink="/dashboard" class="px-4 py-2 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors">
                  Cancel
                </a>
                <button type="submit" [disabled]="pollForm.invalid || isLoading" class="btn-primary w-auto px-6 py-2">
                  <span *ngIf="isLoading" class="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full mr-2"></span>
                  {{ isLoading ? 'Creating...' : 'Create Poll' }}
                </button>
              </div>
            </div>
          </form>
        </div>
      </main>
    </div>
  `
})
export class CreatePollComponent {
  pollForm: FormGroup;
  isLoading = false;
  private pollService = inject(PollService);
  private router = inject(Router);

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
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.error(err);
          this.isLoading = false;
          alert('Failed to create poll.');
        }
      });
    }
  }
}
