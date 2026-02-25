import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { PollVotesResponse, VotesPerDayResponse, VotesPerQuestionResponse } from '../../core/models/result';
import { ResultService } from '../../core/services/result.service';

type ResponseColumnKey = 'voter' | 'date' | 'answersCount' | 'answers';

interface ResponseColumn {
  key: ResponseColumnKey;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-wrapper pt-4 max-w-5xl mx-auto w-full">
      <div class="flex justify-end">
        <a routerLink="/dashboard" class="px-3 py-1.5 text-xs font-semibold rounded-md border border-[var(--border)] hover:bg-[var(--sidebar-hover)] transition-colors">Back to Dashboard</a>
      </div>

      <div *ngIf="errorMessage()" class="sb-error mb-4">{{ errorMessage() }}</div>
      <div *ngIf="loading()" class="py-12 flex justify-center">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--accent)]"></div>
      </div>

      <ng-container *ngIf="!loading()">
        <div class="mb-6 border-b border-[var(--border)]">
          <nav class="-mb-px flex space-x-6" aria-label="Tabs">
            <button (click)="activeTab = 'overview'" [class.border-[var(--accent)]]="activeTab === 'overview'" [class.text-[var(--text)]]="activeTab === 'overview'" [class.border-transparent]="activeTab !== 'overview'" [class.text-[var(--text-soft)]]="activeTab !== 'overview'" class="whitespace-nowrap py-3 px-1 border-b-2 font-bold text-sm hover:text-[var(--text)] transition-colors">Overview</button>
            <button (click)="activeTab = 'responses'" [class.border-[var(--accent)]]="activeTab === 'responses'" [class.text-[var(--text)]]="activeTab === 'responses'" [class.border-transparent]="activeTab !== 'responses'" [class.text-[var(--text-soft)]]="activeTab !== 'responses'" class="whitespace-nowrap py-3 px-1 border-b-2 font-bold text-sm hover:text-[var(--text)] transition-colors">Responses</button>
            <button (click)="activeTab = 'trends'" [class.border-[var(--accent)]]="activeTab === 'trends'" [class.text-[var(--text)]]="activeTab === 'trends'" [class.border-transparent]="activeTab !== 'trends'" [class.text-[var(--text-soft)]]="activeTab !== 'trends'" class="whitespace-nowrap py-3 px-1 border-b-2 font-bold text-sm hover:text-[var(--text)] transition-colors">Trends</button>
          </nav>
        </div>

        <div *ngIf="activeTab === 'overview'" class="space-y-6">
          <div *ngFor="let q of votesPerQuestion(); let i = index" class="sb-surface rounded-xl p-6 border border-[var(--border)]">
            <h3 class="text-base font-bold mb-5 flex items-start gap-2">
              <span class="text-[0.7rem] bg-[var(--bg-soft)] text-[var(--text-soft)] px-1.5 py-0.5 rounded font-mono mt-0.5">Q{{ i + 1 }}</span>
              <span>{{ q.question }}</span>
            </h3>
            <div class="space-y-4">
              <div *ngFor="let a of q.selectedAnswers">
                <div class="flex justify-between text-[0.85rem] font-semibold mb-1.5">
                  <span class="text-[var(--text)]">{{ a.answer }}</span>
                  <span class="text-[var(--text-soft)] text-xs font-mono bg-[var(--bg-soft)] px-1.5 rounded">{{ a.count }} votes</span>
                </div>
                <div class="w-full rounded-full h-2 bg-[var(--bg-soft)] overflow-hidden border border-[var(--border)]">
                  <div class="h-full bg-[var(--accent)] rounded-full transition-all duration-500" [style.width.%]="(a.count / getMaxCount(q.selectedAnswers)) * 100"></div>
                </div>
              </div>
            </div>
          </div>
          <div *ngIf="votesPerQuestion().length === 0" class="sb-empty py-12 text-sm">No analytics data available yet.</div>
        </div>

        <div *ngIf="activeTab === 'responses'" class="space-y-3">
          <div class="sb-stats-grid">
            <article class="sb-stat">
              <p class="sb-stat__label">Total Responses</p>
              <p class="sb-stat__value">{{ responseCount() }}</p>
            </article>
            <article class="sb-stat">
              <p class="sb-stat__label">Answers Captured</p>
              <p class="sb-stat__value">{{ answersCount() }}</p>
            </article>
            <article class="sb-stat">
              <p class="sb-stat__label">Average Answers</p>
              <p class="sb-stat__value">{{ averageAnswers() }}</p>
            </article>
          </div>

          <div class="sb-toolbar">
            <div class="sb-toolbar__group">
              <input class="sb-input sb-search" [value]="searchTerm()" (input)="onSearchInput($event)" placeholder="Search responses by voter or answer" />
              <select class="sb-select" [value]="dateFilter()" (change)="onDateFilterChange($event)">
                <option value="all">All dates</option>
                <option value="today">Today</option>
                <option value="week">Last 7 days</option>
                <option value="month">Last 30 days</option>
              </select>
            </div>

            <div class="sb-columns">
              <button type="button" class="sb-btn-secondary text-xs px-3 py-2" (click)="showColumnsMenu.set(!showColumnsMenu())">Columns</button>
              @if (showColumnsMenu()) {
                <div class="sb-columns__panel">
                  @for (col of allColumns; track col.key) {
                    <label class="sb-columns__option">
                      <input type="checkbox" [checked]="isColumnVisible(col.key)" (change)="toggleColumn(col.key)" />
                      <span class="sb-icon-badge">{{ col.icon }}</span>
                      <span>{{ col.label }}</span>
                    </label>
                  }
                </div>
              }
            </div>
          </div>

          <div class="sb-table-wrap">
            <div class="overflow-x-auto">
              <table class="sb-table">
                <thead>
                  <tr>
                    @if (isColumnVisible('voter')) { <th>Voter</th> }
                    @if (isColumnVisible('date')) { <th>Date</th> }
                    @if (isColumnVisible('answersCount')) { <th>Answers Count</th> }
                    @if (isColumnVisible('answers')) { <th>Answers</th> }
                  </tr>
                </thead>
                <tbody>
                  @for (vote of pagedVotes(); track vote.voteDate + vote.voterName) {
                    <tr>
                      @if (isColumnVisible('voter')) {
                        <td class="font-semibold">{{ vote.voterName }}</td>
                      }
                      @if (isColumnVisible('date')) {
                        <td class="font-mono text-[0.8rem] text-[var(--text-soft)]">{{ vote.voteDate | date:'short' }}</td>
                      }
                      @if (isColumnVisible('answersCount')) {
                        <td class="font-semibold">{{ vote.selectedAnswers.length }}</td>
                      }
                      @if (isColumnVisible('answers')) {
                        <td>
                          <div class="space-y-1">
                            @for (ans of vote.selectedAnswers; track ans.question + ans.answer) {
                              <p class="m-0 text-[0.8rem]"><span class="font-semibold text-[var(--text-soft)]">{{ ans.question }}:</span> {{ ans.answer }}</p>
                            }
                          </div>
                        </td>
                      }
                    </tr>
                  }

                  @if (!pagedVotes().length) {
                    <tr>
                      <td [attr.colspan]="visibleColumns().length">
                        <div class="sb-empty text-sm my-3">No responses match your filters.</div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            @if (responseTotalPages() > 1) {
              <div class="sb-pagination">
                <p class="sb-pagination__info">Page {{ responsePage() }} of {{ responseTotalPages() }}</p>
                <div class="sb-pagination__controls">
                  <button type="button" class="sb-pagination__button" [disabled]="responsePage() === 1" (click)="responsePage.set(responsePage() - 1)">Prev</button>
                  <button type="button" class="sb-pagination__button" [disabled]="responsePage() >= responseTotalPages()" (click)="responsePage.set(responsePage() + 1)">Next</button>
                </div>
              </div>
            }
          </div>
        </div>

        <div *ngIf="activeTab === 'trends'" class="sb-surface rounded-xl p-6 border border-[var(--border)]">
          <h3 class="text-base font-bold mb-6">Votes per Day</h3>
          <div class="space-y-4">
            <div *ngFor="let d of votesPerDay()">
              <div class="flex justify-between text-[0.85rem] font-semibold mb-1.5">
                <span class="font-mono text-xs">{{ d.date | date:'mediumDate' }}</span>
                <span class="text-[var(--text-soft)] text-xs font-mono bg-[var(--bg-soft)] px-1.5 rounded">{{ d.voteCount }} votes</span>
              </div>
              <div class="w-full rounded-full h-2 bg-[var(--bg-soft)] overflow-hidden border border-[var(--border)]">
                <div class="h-full rounded-full transition-all duration-500" style="background: var(--highlight)" [style.width.%]="(d.voteCount / getMaxVoteCount(votesPerDay())) * 100"></div>
              </div>
            </div>
            <div *ngIf="votesPerDay().length === 0" class="sb-empty py-8 text-sm">No trend data available yet.</div>
          </div>
        </div>
      </ng-container>
    </div>
  `
})
export class ResultsComponent implements OnInit {
  protected readonly allColumns: ResponseColumn[] = [
    { key: 'voter', label: 'Voter', icon: 'V' },
    { key: 'date', label: 'Date', icon: 'D' },
    { key: 'answersCount', label: 'Answers Count', icon: 'AC' },
    { key: 'answers', label: 'Answers', icon: 'A' }
  ];

  protected pollId = '';
  protected activeTab: 'overview' | 'responses' | 'trends' = 'overview';

  protected readonly pollVotes = signal<PollVotesResponse | null>(null);
  protected readonly analytics = signal<{ pollId: string; title: string; totalSubmissions: number; questions: Array<{ responses: number }> } | null>(null);
  protected readonly votesPerDay = signal<VotesPerDayResponse[]>([]);
  protected readonly votesPerQuestion = signal<VotesPerQuestionResponse[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly responsePage = signal(1);
  protected readonly responsePageSize = 8;
  protected readonly searchTerm = signal('');
  protected readonly dateFilter = signal<'all' | 'today' | 'week' | 'month'>('all');
  protected readonly visibleColumns = signal<ResponseColumnKey[]>(['voter', 'date', 'answersCount', 'answers']);
  protected readonly showColumnsMenu = signal(false);

  protected readonly filteredVotes = computed(() => {
    const votes = this.pollVotes()?.votes ?? [];
    const term = this.searchTerm().trim().toLowerCase();
    const now = new Date();

    return votes.filter((vote) => {
      const textMatch =
        term.length === 0 ||
        vote.voterName.toLowerCase().includes(term) ||
        vote.selectedAnswers.some((answer) => answer.answer.toLowerCase().includes(term) || answer.question.toLowerCase().includes(term));

      if (!textMatch) {
        return false;
      }

      const dateMode = this.dateFilter();
      if (dateMode === 'all') {
        return true;
      }

      const voteDate = new Date(vote.voteDate);
      const diffMs = now.getTime() - voteDate.getTime();
      const diffDays = diffMs / (1000 * 60 * 60 * 24);

      if (dateMode === 'today') {
        return voteDate.toDateString() === now.toDateString();
      }

      if (dateMode === 'week') {
        return diffDays <= 7;
      }

      return diffDays <= 30;
    });
  });

  protected readonly responseTotalPages = computed(() => Math.max(1, Math.ceil(this.filteredVotes().length / this.responsePageSize)));

  protected readonly pagedVotes = computed(() => {
    const start = (this.responsePage() - 1) * this.responsePageSize;
    return this.filteredVotes().slice(start, start + this.responsePageSize);
  });

  protected readonly responseCount = computed(() => this.analytics()?.totalSubmissions ?? 0);
  protected readonly answersCount = computed(() => (this.analytics()?.questions ?? []).reduce((sum, q) => sum + q.responses, 0));
  protected readonly averageAnswers = computed(() => {
    const responses = this.responseCount();
    if (responses === 0) {
      return '0.0';
    }
    return (this.answersCount() / responses).toFixed(1);
  });

  private readonly route = inject(ActivatedRoute);
  private readonly resultService = inject(ResultService);

  ngOnInit(): void {
    this.pollId = this.route.snapshot.paramMap.get('id') || '';
    if (this.pollId) {
      this.loadData();
    }
  }

  protected onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
    this.responsePage.set(1);
  }

  protected onDateFilterChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.dateFilter.set((target.value as 'all' | 'today' | 'week' | 'month') ?? 'all');
    this.responsePage.set(1);
  }

  protected isColumnVisible(key: ResponseColumnKey): boolean {
    return this.visibleColumns().includes(key);
  }

  protected toggleColumn(key: ResponseColumnKey): void {
    const current = this.visibleColumns();
    if (current.includes(key)) {
      if (current.length === 1) {
        return;
      }
      this.visibleColumns.set(current.filter((item) => item !== key));
      return;
    }
    this.visibleColumns.set([...current, key]);
  }

  protected getMaxCount(answers: Array<{ count: number }>): number {
    return Math.max(...answers.map((a) => a.count), 1);
  }

  protected getMaxVoteCount(days: Array<{ voteCount: number }>): number {
    return Math.max(...days.map((d) => d.voteCount), 1);
  }

  private loadData(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.resultService.getPollVotes(this.pollId).subscribe({
      next: (res) => {
        this.pollVotes.set(res);
        this.responsePage.set(1);
      },
      error: () => this.errorMessage.set('Unable to load response list for this poll.')
    });

    this.resultService.getPollAnalytics(this.pollId).subscribe({
      next: (res) => this.analytics.set(res),
      error: () => this.errorMessage.set('Unable to load analytics summary for this poll.')
    });

    this.resultService.getVotesPerDay(this.pollId).subscribe({
      next: (res) => this.votesPerDay.set(res),
      error: () => this.errorMessage.set('Unable to load votes-per-day analytics.')
    });

    this.resultService.getVotesPerQuestion(this.pollId).subscribe({
      next: (res) => {
        this.votesPerQuestion.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load votes-per-question analytics.');
        this.loading.set(false);
      }
    });
  }
}
