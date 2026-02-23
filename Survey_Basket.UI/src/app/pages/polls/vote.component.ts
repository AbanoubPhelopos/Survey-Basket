import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { VoteService } from '../../core/services/vote.service';
import { QuestionResponse, VoteAnswerRequest } from '../../core/models/vote';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-vote',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="page-wrapper pt-4 max-w-3xl mx-auto w-full">
      <div class="flex justify-end">
        <a routerLink="/dashboard" class="px-3 py-1.5 text-xs font-semibold rounded-md border border-[var(--border)] hover:bg-[var(--sidebar-hover)] transition-colors"> Cancel </a>
      </div>

      <!-- Loading State -->
      <div *ngIf="isLoading()" class="flex justify-center py-12">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--accent)]"></div>
      </div>

      <!-- Error State -->
      <div *ngIf="error()" class="sb-error mb-6 flex gap-3 text-sm">
        <svg class="h-5 w-5 text-red-500 shrink-0" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
        </svg>
        <div>
          <h3 class="font-bold text-red-800">Error</h3>
          <p class="mt-1 text-red-700">{{ error() }}</p>
        </div>
      </div>

      <!-- Form -->
      <form *ngIf="!isLoading() && questions().length > 0" [formGroup]="voteForm" (ngSubmit)="onSubmit()" class="space-y-8">
        <div formArrayName="answers" class="space-y-6">
          <div *ngFor="let question of questions(); let i = index" class="sb-surface rounded-xl overflow-hidden border border-[var(--border)]" [class.border-red-300]="answersArray.controls[i].get('answerId')?.touched && answersArray.controls[i].get('answerId')?.invalid">
            <div class="px-6 py-4 bg-[var(--bg-soft)] border-b border-[var(--border)]">
              <h3 class="text-[0.95rem] font-semibold text-[var(--text)] flex items-start gap-2 leading-snug">
                <span class="text-xs font-mono font-bold text-[var(--text-soft)] bg-black/5 px-1.5 py-0.5 rounded mt-0.5">Q{{i+1}}</span>
                <span>{{ question.content }}</span>
              </h3>
            </div>
            <div class="p-6" [formGroupName]="i">
              <div class="space-y-3">
                <label *ngFor="let answer of question.answers" class="flex items-start gap-3 p-3 rounded-lg border border-[var(--border)] cursor-pointer hover:bg-[var(--sidebar-hover)] hover:border-gray-300 transition-colors has-[:checked]:border-[var(--accent)] has-[:checked]:bg-[var(--accent)]/5">
                  <div class="flex items-center h-5">
                     <input [id]="'q'+i+'a'+answer.id" 
                            type="radio" 
                            formControlName="answerId" 
                            [value]="answer.id"
                            class="w-4 h-4 text-[var(--accent)] bg-white border-gray-300 focus:ring-[var(--accent)]">
                  </div>
                  <div class="text-[0.85rem] font-medium text-[var(--text)] pt-0.5">
                    {{ answer.content }}
                  </div>
                </label>
              </div>
              <input type="hidden" formControlName="questionId">
            </div>
          </div>
        </div>

        <div class="flex flex-col sm:flex-row justify-between items-center gap-4 pt-4 border-t border-[var(--border)]">
           <p class="text-xs text-[var(--text-soft)]">Make sure to answer all questions before submitting.</p>
           <button type="submit" [disabled]="isSubmitting" class="sb-btn-primary w-full sm:w-auto px-6 py-2 shadow-sm">
             {{ isSubmitting ? 'Submitting Vote...' : 'Submit Vote' }}
           </button>
        </div>
      </form>

      <!-- Empty State -->
      <div *ngIf="!isLoading() && !error() && questions().length === 0" class="sb-surface rounded-xl border border-[var(--border)] p-12 text-center text-sm group">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 mx-auto text-gray-300 mb-4 group-hover:text-gray-400 transition-colors">
          <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m3.75 9v6m3-3H9m1.5-12H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z" />
        </svg>

        <p class="text-gray-500 font-medium">This survey has no questions available for you.</p>
        <button routerLink="/dashboard" class="mt-6 px-4 py-2 border border-[var(--border)] rounded-md font-semibold text-sm hover:bg-[var(--sidebar-hover)] transition-colors inline-block">Return to Dashboard</button>
      </div>

    </div>
  `
})
export class VoteComponent implements OnInit {
  voteForm: FormGroup;
  pollId: string = '';

  questions = signal<QuestionResponse[]>([]);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  isSubmitting = false;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private voteService = inject(VoteService);
  private uiFeedback = inject(UiFeedbackService);
  private fb = inject(FormBuilder);

  constructor() {
    this.voteForm = this.fb.group({
      answers: this.fb.array([])
    });
  }

  get answersArray() {
    return this.voteForm.get('answers') as FormArray;
  }

  ngOnInit() {
    this.pollId = this.route.snapshot.paramMap.get('id') || '';
    if (this.pollId) {
      this.loadQuestions();
    } else {
      this.error.set('Invalid Poll ID');
      this.isLoading.set(false);
    }
  }

  loadQuestions() {
    this.voteService.startVote(this.pollId).subscribe({
      next: (data) => {
        this.questions.set(data);
        this.buildForm(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        if (err.status === 403) {
          this.error.set('You are not authorized to vote on this poll. (Are you a Member?)');
        } else {
          this.error.set('Failed to load survey questions.');
        }
        this.isLoading.set(false);
      }
    });
  }

  buildForm(questions: QuestionResponse[]) {
    this.answersArray.clear();
    questions.forEach(q => {
      this.answersArray.push(this.fb.group({
        questionId: [q.id],
        answerId: ['', Validators.required]
      }));
    });
  }

  onSubmit() {
    if (this.voteForm.valid) {
      this.isSubmitting = true;
      const request = {
        answers: this.voteForm.value.answers
      };

      this.voteService.submitVote(this.pollId, request).subscribe({
        next: () => {
          this.uiFeedback.success('Vote submitted', 'Thank you for sharing your answers.');
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.error(err);
          this.error.set('Failed to submit vote.');
          this.isSubmitting = false;
        }
      });
    } else {
      this.voteForm.markAllAsTouched();
      this.uiFeedback.warning('Incomplete answers', 'Please answer every required question.');
    }
  }
}
