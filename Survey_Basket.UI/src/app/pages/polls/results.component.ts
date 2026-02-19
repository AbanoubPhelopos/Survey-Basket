import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ResultService } from '../../core/services/result.service';
import { PollVotesResponse, VotesPerDayResponse, VotesPerQuestionResponse } from '../../core/models/result';

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen py-8 px-4 sm:px-6 lg:px-8">
      <div class="max-w-5xl mx-auto">
        
        <div class="mb-8 flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold">Survey Results</h1>
            <p class="mt-1 text-sm sb-muted">Analytics and response data.</p>
          </div>
          <a routerLink="/dashboard" class="text-sm font-medium">Back to Dashboard</a>
        </div>

        <div *ngIf="errorMessage()" class="sb-error mb-4">{{ errorMessage() }}</div>
        <div *ngIf="loading()" class="sb-empty mb-4">Loading survey analytics...</div>

        <div class="mb-6 border-b border-gray-200">
          <nav class="-mb-px flex space-x-8" aria-label="Tabs">
            <button (click)="activeTab = 'overview'" 
                    [class.border-primary-500]="activeTab === 'overview'"
                    [class.text-primary-600]="activeTab === 'overview'"
                    class="whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300">
              Overview
            </button>
            <button (click)="activeTab = 'responses'" 
                    [class.border-primary-500]="activeTab === 'responses'"
                    [class.text-primary-600]="activeTab === 'responses'"
                    class="whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300">
              Responses
            </button>
            <button (click)="activeTab = 'trends'" 
                    [class.border-primary-500]="activeTab === 'trends'"
                    [class.text-primary-600]="activeTab === 'trends'"
                    class="whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300">
              Trends
            </button>
          </nav>
        </div>

        <div *ngIf="!loading() && activeTab === 'overview'" class="space-y-6">
          <div *ngFor="let q of votesPerQuestion()" class="sb-surface p-6">
            <h3 class="text-lg font-medium mb-4">{{ q.question }}</h3>
            <div class="space-y-3">
              <div *ngFor="let a of q.selectedAnswers">
                <div class="flex justify-between text-sm font-medium mb-1">
                  <span>{{ a.answer }}</span>
                  <span>{{ a.count }} votes</span>
                </div>
                <div class="w-full rounded-full h-2.5" style="background: var(--bg-soft)">
                  <div class="h-2.5 rounded-full" style="background: var(--accent)" [style.width.%]="(a.count / getMaxCount(q.selectedAnswers)) * 100"></div>
                </div>
              </div>
            </div>
          </div>
          <div *ngIf="votesPerQuestion().length === 0" class="sb-empty">No data available.</div>
        </div>

        <div *ngIf="!loading() && activeTab === 'responses'" class="sb-surface overflow-hidden rounded-lg">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Voter</th>
                <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Answers</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr *ngFor="let vote of pollVotes()?.votes">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ vote.voterName }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ vote.voteDate | date:'short' }}</td>
                <td class="px-6 py-4 text-sm text-gray-500">
                  <div *ngFor="let ans of vote.selectedAnswers" class="mb-1">
                    <span class="font-medium">{{ ans.question }}:</span> {{ ans.answer }}
                  </div>
                </td>
              </tr>
              <tr *ngIf="!pollVotes()?.votes?.length">
                <td colspan="3" class="px-6 py-12">
                  <div class="sb-empty">No responses yet.</div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div *ngIf="!loading() && activeTab === 'trends'" class="sb-surface rounded-lg p-6">
          <h3 class="text-lg font-medium mb-4">Votes per Day</h3>
          <div class="space-y-3">
             <div *ngFor="let d of votesPerDay()">
                <div class="flex justify-between text-sm font-medium mb-1">
                  <span>{{ d.date }}</span>
                  <span>{{ d.voteCount }}</span>
                </div>
                <div class="w-full rounded-full h-2.5" style="background: var(--bg-soft)">
                  <div class="h-2.5 rounded-full" style="background: var(--highlight)" [style.width.%]="(d.voteCount / getMaxVoteCount(votesPerDay())) * 100"></div>
                </div>
             </div>
             <div *ngIf="votesPerDay().length === 0" class="sb-empty">No trend data available.</div>
          </div>
        </div>

      </div>
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
      next: (res) => this.pollVotes.set(res),
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
