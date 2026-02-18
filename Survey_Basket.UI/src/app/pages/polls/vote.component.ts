import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { VoteService } from '../../core/services/vote.service';
import { QuestionResponse, VoteAnswerRequest } from '../../core/models/vote';

@Component({
  selector: 'app-vote',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="min-h-screen bg-gray-50 py-10 px-4 sm:px-6 lg:px-8">
      <div class="max-w-3xl mx-auto">
        
        <!-- Header -->
        <div class="mb-8 flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-900">Take Survey</h1>
            <p class="mt-1 text-sm text-gray-500">Please answer all questions below.</p>
          </div>
          <a routerLink="/dashboard" class="text-sm font-medium text-gray-500 hover:text-gray-700">Back to Dashboard</a>
        </div>

        <!-- Loading State -->
        <div *ngIf="isLoading()" class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600"></div>
        </div>

        <!-- Error State -->
        <div *ngIf="error()" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800">Error</h3>
              <div class="mt-2 text-sm text-red-700">
                <p>{{ error() }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Form -->
        <form *ngIf="!isLoading() && questions().length > 0" [formGroup]="voteForm" (ngSubmit)="onSubmit()" class="space-y-6">
          
          <div formArrayName="answers">
            <div *ngFor="let question of questions(); let i = index" class="bg-white shadow rounded-lg overflow-hidden mb-6">
              <div class="px-6 py-5 border-b border-gray-200 bg-gray-50">
                <h3 class="text-lg font-medium leading-6 text-gray-900">
                  <span class="mr-2 text-gray-500">Q{{i+1}}.</span>
                  {{ question.content }}
                </h3>
              </div>
              <div class="px-6 py-6" [formGroupName]="i">
                <div class="space-y-4">
                  <div *ngFor="let answer of question.answers" class="flex items-center">
                    <input [id]="'q'+i+'a'+answer.id" 
                           type="radio" 
                           formControlName="answerId" 
                           [value]="answer.id"
                           class="focus:ring-primary-500 h-4 w-4 text-primary-600 border-gray-300">
                    <label [for]="'q'+i+'a'+answer.id" class="ml-3 block text-sm font-medium text-gray-700 cursor-pointer w-full">
                      {{ answer.content }}
                    </label>
                  </div>
                </div>
                <!-- Hidden QuestionId -->
                <input type="hidden" formControlName="questionId">
              </div>
            </div>
          </div>

          <div class="flex justify-end">
            <button type="submit" [disabled]="voteForm.invalid || isSubmitting" class="btn-primary w-full sm:w-auto px-8">
              <span *ngIf="isSubmitting" class="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full mr-2"></span>
              {{ isSubmitting ? 'Submitting Vote...' : 'Submit Vote' }}
            </button>
          </div>
        </form>

        <!-- Empty State -->
        <div *ngIf="!isLoading() && !error() && questions().length === 0" class="text-center py-12">
          <p class="text-gray-500">This survey has no questions available for you.</p>
          <button routerLink="/dashboard" class="mt-4 text-primary-600 hover:text-primary-500 font-medium">Return to Dashboard</button>
        </div>

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
          alert('Thank you for voting!');
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
      alert('Please answer all questions.');
    }
  }
}
