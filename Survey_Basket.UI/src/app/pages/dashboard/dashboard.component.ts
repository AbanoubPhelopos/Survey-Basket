import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PollService } from '../../core/services/poll.service';
import { RequestFilters, PollResponse, PagedList } from '../../core/models/poll';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],

  template: `
    <div class="page-wrapper pt-4">

      <ng-container *ngIf="isAdmin()">
        <div class="flex items-center justify-between mt-2 mb-4">
          <h2 class="text-lg font-bold"> Manage Polls </h2>
          <a routerLink="/polls/new" class="sb-btn-primary text-xs py-2 px-4 shadow-sm"> 
            <span class="flex items-center gap-1.5">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-4 h-4"><path d="M10.75 4.75a.75.75 0 00-1.5 0v4.5h-4.5a.75.75 0 000 1.5h4.5v4.5a.75.75 0 001.5 0v-4.5h4.5a.75.75 0 000-1.5h-4.5v-4.5z" /></svg>
              Create New
            </span>
          </a>
        </div>

        <!--Admin Layout Grid-->
        <div *ngIf="polls().items.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
          <div *ngFor="let poll of polls().items" class="sb-surface p-5 flex flex-col h-full hover:border-[var(--accent)] transition-all hover:shadow-md group relative overflow-hidden rounded-xl border border-[var(--border)]">
            <!--decorative top bar-->
            <div class="absolute top-0 left-0 right-0 h-1" [ngClass]="poll.isPublished ? 'bg-emerald-500' : 'bg-amber-400'"> </div>

            <div class="flex justify-between items-start mb-3 mt-1">
              <span class="px-2 py-0.5 rounded text-[0.65rem] font-bold uppercase tracking-wider border shadow-sm"
                [ngClass]="poll.isPublished ? 'bg-emerald-50 text-emerald-800 border-emerald-200' : 'bg-amber-50 text-amber-800 border-amber-200'">
                {{ poll.isPublished ? 'LIVE' : 'DRAFT' }}
              </span>
            </div>

            <h3 class="font-bold text-[1.05rem] mb-2 leading-snug group-hover:text-[var(--accent)] transition-colors text-[var(--text)]"> {{ poll.title }}</h3>
            <p class="text-[0.85rem] text-[var(--text-soft)] mb-6 line-clamp-3 leading-relaxed flex-1"> {{ poll.summary }}</p>

            <div class="grid grid-cols-2 gap-2 mt-auto border-t border-[var(--border)] pt-4">
              <a [routerLink]="['/polls', poll.id, 'edit']" class="px-3 py-1.5 bg-[var(--bg-soft)] hover:bg-[var(--sidebar-hover)] text-[0.75rem] font-bold rounded-md transition-all border border-[var(--border)] text-center text-[var(--text)] shadow-sm"> Edit </a>
              <a [routerLink]="['/polls', poll.id, 'results']" class="px-3 py-1.5 bg-[var(--accent)]/10 text-[var(--accent)] text-[0.75rem] font-bold rounded-md transition-all border border-[var(--accent)]/20 text-center hover:bg-[var(--accent)] hover:text-white shadow-sm"> Stats </a>
              <button (click)="deletePoll(poll.id)" class="col-span-2 py-2 mt-1 text-red-600 hover:text-red-700 hover:bg-red-50 text-[0.7rem] font-bold uppercase tracking-wider transition-all rounded-md"> Delete Survey </button>
            </div>
          </div>
        </div>

        <!--Admin Empty State-->
        <div *ngIf="polls().items.length === 0" class="sb-surface rounded-xl border border-[var(--border)] p-12 text-center">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 mx-auto text-gray-300 mb-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m3.75 9v6m3-3H9m1.5-12H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z" />
            </svg>
          <p class="text-gray-500 text-sm font-medium">No polls created yet.</p>
        </div>
      </ng-container>

      <!--MEMBER VIEW-->
      <ng-container *ngIf="!isAdmin()">
        <div class="mt-2 mb-4 border-b border-[var(--border)] pb-2">
          <h2 class="text-lg font-bold"> Active Surveys </h2>
        </div>

        <div *ngIf="availablePolls().length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          <div *ngFor="let poll of availablePolls()" class="sb-surface p-6 flex flex-col group hover:border-[var(--accent)] transition-all hover:shadow-md rounded-xl border border-[var(--border)]">
            <h3 class="font-bold text-lg mb-2 group-hover:text-[var(--accent)] transition-colors leading-snug text-[var(--text)]"> {{ poll.title }}</h3>
            <p class="text-[0.85rem] text-[var(--text-soft)] mb-6 leading-relaxed flex-1 line-clamp-3"> {{ poll.summary }}</p>
            <a [routerLink]="['/polls', poll.id, 'vote']" class="sb-btn-primary w-full py-2.5 text-center block text-sm shadow-sm mt-auto">
              Take Survey
            </a>
          </div>
        </div>

        <div *ngIf="availablePolls().length === 0" class="sb-surface rounded-xl border border-[var(--border)] p-12 text-center">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 mx-auto text-gray-300 mb-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M10.125 2.25h-4.5c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125v-9M10.125 2.25h.375a9 9 0 019 9v.375M10.125 2.25A3.375 3.375 0 0113.5 5.625v1.5c0 .621.504 1.125 1.125 1.125h1.5a3.375 3.375 0 013.375 3.375M9 15l2.25 2.25L15 12" />
            </svg>
          <p class="text-gray-500 text-sm font-medium">Check back later for new surveys!</p>
        </div>
      </ng-container>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  pollService = inject(PollService);

  polls = signal<PagedList<PollResponse>>({ items: [], pageNumber: 1, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false });
  availablePolls = signal<PollResponse[]>([]);

  filters: RequestFilters = { pageNumber: 1, pageSize: 9, sortColumn: 'CreatedOn', sortDirection: 'DESC' };

  ngOnInit() {
    this.refreshData();
  }

  isAdmin() {
    return this.authService.hasRole('Admin');
  }

  refreshData() {
    if (this.isAdmin()) {
      this.loadPolls();
    } else {
      this.loadCurrentPolls();
    }
  }

  loadPolls() {
    this.pollService.getPolls(this.filters).subscribe(result => this.polls.set(result));
  }

  loadCurrentPolls() {
    this.pollService.getCurrentPolls().subscribe(result => this.availablePolls.set(result));
  }

  changePage(page: number) {
    this.filters.pageNumber = page;
    this.loadPolls();
  }

  deletePoll(id: string) {
    if (confirm('Permanently delete this survey?')) {
      this.pollService.deletePoll(id).subscribe(() => this.loadPolls());
    }
  }

  getInitials(first?: string, last?: string): string {
    return ((first?.[0] || '') + (last?.[0] || '')).toUpperCase() || 'SA';
  }
}
