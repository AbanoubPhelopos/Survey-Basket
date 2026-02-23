import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PollService } from '../../core/services/poll.service';
import { QuestionService } from '../../core/services/question.service';
import { Question, QuestionRequest, QuestionType } from '../../core/models/question';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-edit-poll',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="page-wrapper pt-4">
      <div class="flex items-center justify-end gap-3">
        <a routerLink="/dashboard" class="px-3 py-1.5 text-xs font-semibold rounded-md border border-[var(--border)] hover:bg-[var(--sidebar-hover)] transition-colors"> Cancel </a>
        <button (click)="savePoll()" [disabled]="pollForm.invalid || isSaving" class="sb-btn-primary py-1.5 text-xs">
          {{ isSaving ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 items-start">
        
        <!-- Sidebar / Tabs -->
        <div class="lg:col-span-1 sticky top-6">
          <nav class="flex flex-col gap-1 p-2 sb-surface rounded-xl border border-[var(--border)]" aria-label="Sidebar">
            <button (click)="activeTab = 'settings'" 
                    [class.bg-[var(--accent)]]="activeTab === 'settings'"
                    [class.text-white]="activeTab === 'settings'"
                    [class.hover:bg-[var(--sidebar-hover)]]="activeTab !== 'settings'"
                    [class.text-[var(--text-soft)]]="activeTab !== 'settings'"
                    class="group flex items-center px-4 py-2.5 text-sm font-semibold rounded-lg w-full transition-colors">
              <span class="truncate">Poll Settings</span>
            </button>
            <button (click)="activeTab = 'questions'" 
                    [class.bg-[var(--accent)]]="activeTab === 'questions'"
                    [class.text-white]="activeTab === 'questions'"
                    [class.hover:bg-[var(--sidebar-hover)]]="activeTab !== 'questions'"
                    [class.text-[var(--text-soft)]]="activeTab !== 'questions'"
                    class="group flex items-center justify-between px-4 py-2.5 text-sm font-semibold rounded-lg w-full transition-colors">
              <span class="truncate">Questions</span>
              <span class="bg-black/10 px-2 py-0.5 rounded-full text-[0.65rem]">{{ questions().length }}</span>
            </button>
          </nav>
        </div>

        <!-- Content -->
        <div class="lg:col-span-3">
          
          <!-- Settings Tab -->
          <div *ngIf="activeTab === 'settings'" class="sb-surface rounded-xl p-6 lg:p-8 space-y-6">
            <h2 class="text-lg font-bold border-b border-[var(--border)] pb-2 mb-4">General Information</h2>
            <form [formGroup]="pollForm" class="space-y-6">
              <label class="block">
                <span class="block text-sm font-semibold mb-1.5">Title</span>
                <input type="text" formControlName="title" class="sb-input">
              </label>
              
              <label class="block">
                <span class="block text-sm font-semibold mb-1.5">Summary</span>
                <textarea formControlName="summary" rows="3" class="sb-input"></textarea>
              </label>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <label class="block">
                  <span class="block text-sm font-semibold mb-1.5">Start Date</span>
                  <input type="date" formControlName="startedAt" class="sb-input">
                </label>
                <label class="block">
                  <span class="block text-sm font-semibold mb-1.5">End Date</span>
                  <input type="date" formControlName="endedAt" class="sb-input">
                </label>
              </div>

              <label class="flex items-center gap-3 p-4 border border-[var(--border)] rounded-lg bg-[var(--bg-soft)] cursor-pointer hover:bg-[var(--sidebar-hover)] transition-colors">
                  <input type="checkbox" formControlName="isPublished" class="w-4 h-4 rounded border-gray-300 text-[var(--accent)] focus:ring-[var(--accent)]">
                  <div>
                    <span class="block text-sm font-bold text-[var(--text)]">Published</span>
                    <span class="block text-xs text-[var(--text-soft)] mt-0.5">Make this poll visible to assigned users</span>
                  </div>
              </label>
            </form>
          </div>

          <!-- Questions Tab -->
          <div *ngIf="activeTab === 'questions'" class="space-y-6">
            
            <!-- Add Question Form -->
            <div class="sb-surface rounded-xl p-6 lg:p-8 border-l-4 border-l-[var(--accent)]">
              <h3 class="text-lg font-bold mb-6">Add New Question</h3>
              <form [formGroup]="questionForm" (ngSubmit)="addQuestion()" class="space-y-5">
                <label class="block">
                  <span class="block text-sm font-semibold mb-1.5">Question Text</span>
                  <input type="text" formControlName="content" class="sb-input" placeholder="e.g. What is your favorite color?">
                </label>

                <div class="grid grid-cols-1 sm:grid-cols-12 gap-4">
                  <label class="block sm:col-span-5">
                    <span class="block text-sm font-semibold mb-1.5">Question Type</span>
                    <select formControlName="type" class="sb-input" (change)="onQuestionTypeChange()">
                      <option *ngFor="let type of questionTypeOptions" [value]="type.value">{{ type.label }}</option>
                    </select>
                  </label>
                  <label class="block sm:col-span-3">
                    <span class="block text-sm font-semibold mb-1.5">Display Order</span>
                    <input type="number" formControlName="displayOrder" class="sb-input" min="1">
                  </label>
                  <label class="flex items-center gap-2 sm:col-span-4 mt-6">
                    <input type="checkbox" formControlName="isRequired" class="w-4 h-4 rounded border-gray-300 text-[var(--accent)] focus:ring-[var(--accent)]">
                    <span class="text-sm font-semibold">Required Question</span>
                  </label>
                </div>
                
                <div formArrayName="answers" class="space-y-3 bg-[var(--bg-soft)] p-4 rounded-lg border border-[var(--border)]" *ngIf="requiresAnswerOptions()">
                  <label class="block text-sm font-semibold">Answer Options</label>
                  
                  <div class="space-y-2">
                    <div *ngFor="let ans of answersArray.controls; let i=index" class="flex items-center gap-2">
                      <div class="flex-1">
                        <input [formControlName]="i" type="text" class="sb-input py-1.5 text-sm" [placeholder]="'Option ' + (i + 1)">
                      </div>
                      <button type="button" (click)="removeAnswer(i)" class="p-1.5 text-red-500 hover:text-red-700 hover:bg-red-50 rounded transition-colors" *ngIf="answersArray.length > 2" aria-label="Remove answer">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                          <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                        </svg>
                      </button>
                    </div>
                  </div>
                  
                  <button type="button" (click)="addAnswerField()" class="text-xs font-bold text-[var(--accent)] hover:text-blue-800 transition-colors uppercase tracking-wider inline-flex items-center gap-1 mt-2">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" /></svg>
                    Add Option
                  </button>
                </div>

                <div *ngIf="!requiresAnswerOptions()" class="sb-empty py-4 text-xs">
                   This question type does not require manual answer options.
                </div>

                <div *ngIf="questionError()" class="sb-error">{{ questionError() }}</div>

                <div class="flex justify-end pt-2 border-t border-[var(--border)]">
                  <button type="submit" [disabled]="questionForm.invalid || isAddingQuestion" class="sb-btn-primary">
                    {{ isAddingQuestion ? 'Adding...' : 'Add Question' }}
                  </button>
                </div>
              </form>
            </div>

            <!-- Question List -->
             <div class="space-y-4">
              <h3 class="font-bold text-lg mb-4">Current Questions</h3>
              
              <div *ngFor="let q of questions(); let i = index" class="sb-surface rounded-xl p-5 border border-[var(--border)] relative group hover:border-[var(--accent)] transition-colors">
                <div class="flex justify-between items-start gap-4">
                  <div class="flex-1">
                    <div class="flex items-center gap-2 mb-1.5">
                      <span class="text-xs font-mono font-bold text-[var(--text-soft)] bg-[var(--bg-soft)] px-1.5 rounded">Q{{i+1}}</span>
                      <span class="inline-flex items-center px-2 py-0.5 rounded text-[0.6rem] font-bold uppercase tracking-wider bg-slate-100 text-slate-700 border border-slate-200">
                        {{ normalizeQuestionTypeLabel(q.type) }}
                      </span>
                      <span *ngIf="q.isRequired" class="text-[10px] uppercase font-bold text-red-600 tracking-wider">Required</span>
                    </div>
                    
                    <h4 class="text-[0.95rem] font-semibold text-[var(--text)]">{{ q.content }}</h4>
                    
                    <ul class="mt-3 space-y-1.5 w-full max-w-md" *ngIf="q.answers && q.answers.length > 0">
                      <li *ngFor="let a of q.answers; let ansIdx = index" class="text-[0.8rem] text-[var(--text-soft)] flex items-start gap-2 bg-[var(--bg-soft)] py-1.5 px-3 rounded border border-[var(--border)]">
                        <span class="font-mono text-[0.65rem] opacity-50 mt-0.5">{{ String.fromCharCode(65 + ansIdx) }}.</span>
                        <span>{{ a.content }}</span>
                      </li>
                    </ul>
                  </div>
                  
                  <div class="flex flex-col items-end gap-2 shrink-0">
                    <button (click)="toggleQuestionStatus(q)" 
                            [class.bg-emerald-50]="q.isActive" [class.text-emerald-700]="q.isActive" [class.border-emerald-200]="q.isActive"
                            [class.bg-gray-50]="!q.isActive" [class.text-gray-600]="!q.isActive" [class.border-gray-200]="!q.isActive"
                            class="px-2.5 py-1 text-[0.65rem] font-bold uppercase tracking-wider rounded border transition-colors focus:ring-2 focus:ring-offset-1 focus:ring-[var(--accent)] outline-none">
                      {{ q.isActive ? 'Active' : 'Inactive' }}
                    </button>
                  </div>
                </div>
              </div>
              
              <div *ngIf="questions().length === 0" class="sb-empty py-12 text-sm">
                No questions yet. Add one above!
              </div>
            </div>

          </div>

        </div>
      </div>
    </div>
  `
})
export class EditPollComponent implements OnInit {
  protected readonly String = String;
  pollId: string = '';
  activeTab: 'settings' | 'questions' = 'settings';

  pollForm: FormGroup;
  questionForm: FormGroup;

  questions = signal<Question[]>([]);
  questionError = signal<string | null>(null);
  isSaving = false;
  isAddingQuestion = false;

  readonly questionTypeOptions: Array<{ value: QuestionType; label: string }> = [
    { value: 'SingleChoice', label: 'Single choice (MCQ)' },
    { value: 'MultipleChoice', label: 'Multiple choice' },
    { value: 'TrueFalse', label: 'True / False' },
    { value: 'Number', label: 'Number' },
    { value: 'Text', label: 'Text answer' },
    { value: 'FileUpload', label: 'File upload' },
    { value: 'Country', label: 'Country' }
  ];

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private pollService = inject(PollService);
  private questionService = inject(QuestionService);
  private uiFeedback = inject(UiFeedbackService);
  private fb = inject(FormBuilder);

  constructor() {
    this.pollForm = this.fb.group({
      title: ['', Validators.required],
      summary: ['', Validators.required],
      startedAt: ['', Validators.required],
      endedAt: [''],
      isPublished: [false]
    });

    this.questionForm = this.fb.group({
      content: ['', Validators.required],
      type: ['SingleChoice', Validators.required],
      isRequired: [true],
      displayOrder: [1, [Validators.required, Validators.min(1)]],
      answers: this.fb.array([
        this.fb.control('', Validators.required),
        this.fb.control('', Validators.required)
      ])
    });
  }

  get answersArray() {
    return this.questionForm.get('answers') as FormArray;
  }

  ngOnInit() {
    this.pollId = this.route.snapshot.paramMap.get('id') || '';
    if (this.pollId) {
      this.loadPoll();
      this.loadQuestions();
    }
  }

  loadPoll() {
    this.pollService.getPoll(this.pollId).subscribe(poll => {
      this.pollForm.patchValue({
        title: poll.title,
        summary: poll.summary,
        startedAt: poll.startedAt,
        endedAt: poll.endedAt,
        isPublished: poll.isPublished
      });
    });
  }

  loadQuestions() {
    this.questionService.getQuestions(this.pollId).subscribe(qs => {
      this.questions.set(qs);
    });
  }

  savePoll() {
    if (this.pollForm.valid) {
      this.isSaving = true;
      this.pollService.updatePoll(this.pollId, this.pollForm.value).subscribe({
        next: () => {
          this.isSaving = false;
          this.uiFeedback.success('Survey updated', 'Your changes were saved successfully.');
        },
        error: () => {
          this.isSaving = false;
          this.uiFeedback.error('Update failed', 'Unable to save survey changes.');
        }
      });
    }
  }

  // Question Methods
  addAnswerField() {
    this.answersArray.push(this.fb.control('', Validators.required));
  }

  removeAnswer(index: number) {
    this.answersArray.removeAt(index);
  }

  addQuestion() {
    if (this.questionForm.valid) {
      this.isAddingQuestion = true;
      this.questionError.set(null);

      const request = this.buildQuestionRequest();
      if (!request) {
        this.isAddingQuestion = false;
        return;
      }

      this.questionService.addQuestion(this.pollId, request).subscribe({
        next: () => {
          this.isAddingQuestion = false;
          this.questionError.set(null);
          this.questionForm.reset();
          this.questionForm.patchValue({ type: 'SingleChoice', isRequired: true, displayOrder: this.questions().length + 1 });
          this.answersArray.clear();
          this.addAnswerField();
          this.addAnswerField();

          this.loadQuestions(); // Reload list
          this.uiFeedback.success('Question added', 'The question was added to the survey.');
        },
        error: (err) => {
          console.error(err);
          this.isAddingQuestion = false;
          this.questionError.set(err?.error?.error?.[1] || err?.error?.title || 'Failed to add question.');
        }
      });
    }
  }

  requiresAnswerOptions(): boolean {
    const type = this.questionForm.get('type')?.value as QuestionType;
    return ['SingleChoice', 'MultipleChoice', 'Country'].includes(type);
  }

  onQuestionTypeChange(): void {
    if (this.requiresAnswerOptions()) {
      if (this.answersArray.length < 2) {
        this.answersArray.clear();
        this.addAnswerField();
        this.addAnswerField();
      }
      return;
    }

    this.answersArray.clear();
  }

  normalizeQuestionTypeLabel(type: QuestionType | number): string {
    const map: Record<string, string> = {
      SingleChoice: 'Single choice',
      MultipleChoice: 'Multiple choice',
      TrueFalse: 'True/False',
      Number: 'Number',
      Text: 'Text',
      FileUpload: 'File upload',
      Country: 'Country',
      '1': 'Single choice',
      '2': 'Multiple choice',
      '3': 'True/False',
      '4': 'Number',
      '5': 'Text',
      '6': 'File upload',
      '7': 'Country'
    };
    return map[String(type)] ?? String(type);
  }

  private mapQuestionTypeToApiValue(type: QuestionType): number {
    const map: Record<QuestionType, number> = {
      SingleChoice: 1,
      MultipleChoice: 2,
      TrueFalse: 3,
      Number: 4,
      Text: 5,
      FileUpload: 6,
      Country: 7
    };

    return map[type];
  }

  private buildQuestionRequest(): QuestionRequest | null {
    const raw = this.questionForm.getRawValue();
    const type = raw.type as QuestionType;

    let answers = (raw.answers ?? [])
      .map((x: string) => x?.trim())
      .filter((x: string) => !!x);

    answers = Array.from(new Set(answers));

    if (!this.requiresAnswerOptions()) {
      answers = [];
    }

    if (type === 'Country' && answers.length < 1) {
      this.questionError.set('Country question requires at least one option.');
      return null;
    }

    if ((type === 'SingleChoice' || type === 'MultipleChoice') && answers.length < 2) {
      this.questionError.set('Choice questions require at least two unique options.');
      return null;
    }

    return {
      content: raw.content,
      type: this.mapQuestionTypeToApiValue(type),
      isRequired: !!raw.isRequired,
      displayOrder: Number(raw.displayOrder || 1),
      answers
    };
  }

  toggleQuestionStatus(q: Question) {
    this.questionService.toggleStatus(this.pollId, q.id).subscribe(() => {
      q.isActive = !q.isActive;
    });
  }
}
