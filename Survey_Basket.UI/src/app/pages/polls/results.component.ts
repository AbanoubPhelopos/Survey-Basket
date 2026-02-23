import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ResultService } from '../../core/services/result.service';
import { PollVotesResponse, VotesPerDayResponse, VotesPerQuestionResponse } from '../../core/models/result';

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
            <button (click)="activeTab = 'overview'" 
                    [class.border-[var(--accent)]]="activeTab === 'overview'"
                    [class.text-[var(--text)]]="activeTab === 'overview'"
                    [class.border-transparent]="activeTab !== 'overview'"
                    [class.text-[var(--text-soft)]]="activeTab !== 'overview'"
                    class="whitespace-nowrap py-3 px-1 border-b-2 font-bold text-sm hover:text-[var(--text)] transition-colors">
              Overview
            </button>
            <button (click)="activeTab = 'responses'" 
                    [class.border-[var(--accent)]]="activeTab === 'responses'"
                    [class.text-[var(--text)]]="activeTab === 'responses'"
                    [class.border-transparent]="activeTab !== 'responses'"
                    [class.text-[var(--text-soft)]]="activeTab !== 'responses'"
                    class="whitespace-nowrap py-3 px-1 border-b-2 font-bold text-sm hover:text-[var(--text)] transition-colors">
              Responses
            </button>
            <button (click)="activeTab = 'trends'" 
                    [class.border-[var(--accent)]]="activeTab === 'trends'"
                    [class.text-[var(--text)]]="activeTab === 'trends'"
                    [class.border-transparent]="activeTab !== 'trends'"
                    [class.text-[var(--text-soft)]]="activeTab !== 'trends'"
                    class="whitespace-nowrap py-3 px-1 border-b-2 font-bold text-sm hover:text-[var(--text)] transition-colors">
              Trends
            </button>
          </nav>
        </div>

        <div *ngIf="activeTab === 'overview'" class="space-y-6">
          <div *ngFor="let q of votesPerQuestion(); let i=index" class="sb-surface rounded-xl p-6 border border-[var(--border)]">
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
                  <div class="h-full bg-[var(--accent)] rounded-full transition-all duration-500 relative" [style.width.%]="(a.count / getMaxCount(q.selectedAnswers)) * 100">
                     <div class="absolute inset-0 bg-white/20 w-full" style="background-image: linear-gradient(45deg, rgba(255, 255, 255, 0.15) 25%, transparent 25%, transparent 50%, rgba(255, 255, 255, 0.15) 50%, rgba(255, 255, 255, 0.15) 75%, transparent 75%, transparent); background-size: 1rem 1rem;"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div *ngIf="votesPerQuestion().length === 0" class="sb-empty py-12 text-sm">No analytics data available yet.</div>
        </div>

        <div *ngIf="activeTab === 'responses'" class="sb-table-wrap">
          <div class="overflow-x-auto">
            <table class="sb-table">
              <thead>
                <tr>
                  <th scope="col" class="px-6 py-3 tracking-wider">Voter</th>
                  <th scope="col" class="px-6 py-3 tracking-wider">Date</th>
                  <th scope="col" class="px-6 py-3 tracking-wider">Answers</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let vote of pagedVotes()">
                  <td class="px-6 py-4 whitespace-nowrap font-semibold">{{ vote.voterName }}</td>
                  <td class="px-6 py-4 whitespace-nowrap font-mono text-[0.8rem] text-[var(--text-soft)]">{{ vote.voteDate | date:'short' }}</td>
                  <td class="px-6 py-4">
                    <div class="space-y-2">
                      <div *ngFor="let ans of vote.selectedAnswers; let idx=index" class="text-[0.85rem] leading-snug">
                         <span class="font-bold text-[var(--text-soft)] text-[0.7rem] uppercase mr-1">Q{{idx+1}}</span>
                         <span>{{ ans.answer }}</span>
                      </div>
                    </div>
                  </td>
                </tr>
                <tr *ngIf="!pagedVotes().length">
                  <td colspan="3" class="px-6 py-12">
                    <div class="sb-empty text-sm">No responses yet.</div>
                  </td>
                </tr>
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
  pollId: string = '';
  activeTab: 'overview' | 'responses' | 'trends' = 'overview';

  pollVotes = signal<PollVotesResponse | null>(null);
  votesPerDay = signal<VotesPerDayResponse[]>([]);
  votesPerQuestion = signal<VotesPerQuestionResponse[]>([]);
  loading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);
  responsePage = signal(1);
  readonly responsePageSize = 8;
  readonly responseTotalPages = computed(() => {
    const total = this.pollVotes()?.votes?.length ?? 0;
    return Math.max(1, Math.ceil(total / this.responsePageSize));
  });
  readonly pagedVotes = computed(() => {
    const votes = this.pollVotes()?.votes ?? [];
    const start = (this.responsePage() - 1) * this.responsePageSize;
    return votes.slice(start, start + this.responsePageSize);
  });

  private route = inject(ActivatedRoute);
  private resultService = inject(ResultService);

  ngOnInit() {
    this.pollId = this.route.snapshot.paramMap.get('id') || '';
    if (this.pollId) {
      this.loadData();
    }
  }

  loadData() {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.resultService.getPollVotes(this.pollId).subscribe({
      next: (res) => {
        this.pollVotes.set(res);
        this.responsePage.set(1);
      },
      error: () => this.errorMessage.set('Unable to load response list for this poll.')
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

  getMaxCount(answers: any[]): number {
    return Math.max(...answers.map(a => a.count), 1);
  }

  getMaxVoteCount(days: any[]): number {
    return Math.max(...days.map(d => d.voteCount), 1);
  }
}
