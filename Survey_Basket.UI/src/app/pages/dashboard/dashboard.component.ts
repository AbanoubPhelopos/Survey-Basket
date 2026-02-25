import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { PollResponse, RequestFilters } from '../../core/models/poll';
import { PollService } from '../../core/services/poll.service';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';
import { PagedList } from '../../core/models/service-result';

type PollColumnKey = 'title' | 'status' | 'startDate' | 'endDate' | 'votes' | 'answers';

interface PollColumn {
  key: PollColumnKey;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-wrapper pt-4">
      <ng-container *ngIf="isAdmin()">
        <div class="sb-stats-grid">
          <article class="sb-stat">
            <p class="sb-stat__label">Total Polls</p>
            <p class="sb-stat__value">{{ pollStats().totalPolls }}</p>
          </article>
          <article class="sb-stat">
            <p class="sb-stat__label">Active Polls</p>
            <p class="sb-stat__value">{{ activePollCount() }}</p>
          </article>
          <article class="sb-stat">
            <p class="sb-stat__label">Draft Polls</p>
            <p class="sb-stat__value">{{ draftPollCount() }}</p>
          </article>
          <article class="sb-stat">
            <p class="sb-stat__label">Total Votes</p>
            <p class="sb-stat__value">{{ pollStats().votesCount }}</p>
          </article>
          <article class="sb-stat">
            <p class="sb-stat__label">Answers Captured</p>
            <p class="sb-stat__value">{{ answersCount() }}</p>
          </article>
        </div>

        <div class="sb-toolbar">
          <div class="sb-toolbar__group">
            <input
              class="sb-input sb-search"
              [value]="searchTerm()"
              (input)="onSearchInput($event)"
              (keydown.enter)="applySearch()"
              placeholder="Search polls by title or summary" />

            <select class="sb-select" [value]="statusFilter()" (change)="onStatusChange($event)">
              <option value="all">All statuses</option>
              <option value="active">Active only</option>
              <option value="draft">Draft only</option>
            </select>

            <button type="button" class="sb-btn-secondary text-xs px-3 py-2" (click)="applySearch()">Search</button>
          </div>

          <div class="sb-toolbar__group">
            <a routerLink="/polls/new" class="sb-btn-primary text-xs py-2 px-3">+ New Poll</a>
            <div class="sb-columns">
              <button type="button" class="sb-btn-secondary text-xs px-3 py-2" (click)="toggleColumnsMenu()">Columns</button>
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
        </div>

        <div class="sb-table-wrap">
          <div class="overflow-x-auto">
            <table class="sb-table">
              <thead>
                <tr>
                  @if (isColumnVisible('title')) { <th>Title</th> }
                  @if (isColumnVisible('status')) { <th>Status</th> }
                  @if (isColumnVisible('startDate')) { <th>Start</th> }
                  @if (isColumnVisible('endDate')) { <th>End</th> }
                  @if (isColumnVisible('votes')) { <th>Votes</th> }
                  @if (isColumnVisible('answers')) { <th>Answers</th> }
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (poll of filteredPolls(); track poll.id) {
                  <tr>
                    @if (isColumnVisible('title')) {
                      <td>
                        <p class="m-0 font-semibold">{{ poll.title }}</p>
                        <p class="m-0 mt-1 text-xs text-[var(--text-soft)] line-clamp-2">{{ poll.summary }}</p>
                      </td>
                    }
                    @if (isColumnVisible('status')) {
                      <td>
                        <span class="inline-flex items-center px-2 py-1 rounded text-[0.65rem] font-bold uppercase tracking-wider border"
                          [ngClass]="poll.isPublished ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-amber-50 text-amber-700 border-amber-200'">
                          {{ poll.isPublished ? 'Active' : 'Draft' }}
                        </span>
                      </td>
                    }
                    @if (isColumnVisible('startDate')) {
                      <td class="text-xs font-mono text-[var(--text-soft)]">{{ poll.startedAt | date:'mediumDate' }}</td>
                    }
                    @if (isColumnVisible('endDate')) {
                      <td class="text-xs font-mono text-[var(--text-soft)]">{{ poll.endedAt ? (poll.endedAt | date:'mediumDate') : '--' }}</td>
                    }
                    @if (isColumnVisible('votes')) {
                      <td class="font-semibold">--</td>
                    }
                    @if (isColumnVisible('answers')) {
                      <td class="font-semibold">--</td>
                    }
                    <td>
                      <div class="flex items-center gap-2">
                        <a [routerLink]="['/polls', poll.id, 'edit']" class="sb-btn-secondary text-xs px-2.5 py-1.5">Manage</a>
                        <a [routerLink]="['/polls', poll.id, 'results']" class="sb-btn-secondary text-xs px-2.5 py-1.5">Results</a>
                        <button type="button" (click)="deletePoll(poll.id)" class="sb-btn-secondary text-xs px-2.5 py-1.5 text-red-700">Delete</button>
                      </div>
                    </td>
                  </tr>
                }
                @if (!filteredPolls().length) {
                  <tr>
                    <td [attr.colspan]="visibleColumns().length + 1">
                      <div class="sb-empty text-sm my-3">No polls match your filters.</div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <div class="sb-pagination">
            <p class="sb-pagination__info">Page {{ polls().pageNumber }} of {{ polls().totalPages || 1 }}</p>
            <div class="sb-pagination__controls">
              <button type="button" class="sb-pagination__button" (click)="changePage(polls().pageNumber - 1)" [disabled]="!polls().hasPreviousPage">Prev</button>
              <button type="button" class="sb-pagination__button" (click)="changePage(polls().pageNumber + 1)" [disabled]="!polls().hasNextPage">Next</button>
            </div>
          </div>
        </div>
      </ng-container>

      <ng-container *ngIf="!isAdmin()">
        <p class="mt-2 text-sm font-bold text-[var(--text)]">Active Surveys</p>

        <div *ngIf="availablePolls().length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          <div *ngFor="let poll of availablePolls()" class="sb-surface p-6 flex flex-col group hover:border-[var(--accent)] transition-all hover:shadow-md rounded-xl border border-[var(--border)]">
            <h3 class="font-bold text-lg mb-2 group-hover:text-[var(--accent)] transition-colors leading-snug text-[var(--text)]">{{ poll.title }}</h3>
            <p class="text-[0.85rem] text-[var(--text-soft)] mb-6 leading-relaxed flex-1 line-clamp-3">{{ poll.summary }}</p>
            <a [routerLink]="['/polls', poll.id, 'vote']" class="sb-btn-primary w-full py-2.5 text-center block text-sm shadow-sm mt-auto">Take Survey</a>
          </div>
        </div>

        <div *ngIf="availablePolls().length === 0" class="sb-surface rounded-xl border border-[var(--border)] p-12 text-center">
          <p class="text-gray-500 text-sm font-medium">Check back later for new surveys!</p>
        </div>
      </ng-container>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  protected readonly allColumns: PollColumn[] = [
    { key: 'title', label: 'Title', icon: 'T' },
    { key: 'status', label: 'Status', icon: 'S' },
    { key: 'startDate', label: 'Start Date', icon: 'SD' },
    { key: 'endDate', label: 'End Date', icon: 'ED' },
    { key: 'votes', label: 'Votes', icon: 'V' },
    { key: 'answers', label: 'Answers', icon: 'A' }
  ];

  protected readonly authService = inject(AuthService);
  protected readonly pollService = inject(PollService);
  protected readonly uiFeedback = inject(UiFeedbackService);

  protected readonly polls = signal<PagedList<PollResponse>>({
    items: [],
    pageNumber: 1,
    totalPages: 0,
    totalCount: 0,
    hasPreviousPage: false,
    hasNextPage: false
  });

  protected readonly availablePolls = signal<PollResponse[]>([]);
  protected readonly searchTerm = signal('');
  protected readonly statusFilter = signal<'all' | 'active' | 'draft'>('all');
  protected readonly showColumnsMenu = signal(false);
  protected readonly visibleColumns = signal<PollColumnKey[]>(['title', 'status', 'startDate', 'endDate']);
  protected readonly pollStats = signal({ totalPolls: 0, activePolls: 0, draftPolls: 0, votesCount: 0, answersCount: 0 });

  protected filters: RequestFilters = { pageNumber: 1, pageSize: 8, sortColumn: 'CreatedOn', sortDirection: 'DESC' };

  protected readonly quickActions = computed(() => {
    const all = [
      { route: '/dashboard', label: 'Dashboard', icon: 'DB' },
      { route: '/polls/new', label: 'Polls', icon: 'PL', roles: ['Admin', 'SystemAdmin', 'PartnerCompany'] },
      { route: '/users', label: 'Users', icon: 'US', roles: ['Admin', 'SystemAdmin'] },
      { route: '/admin/companies', label: 'Companies', icon: 'CO', roles: ['Admin', 'SystemAdmin'] },
      { route: '/company/users', label: 'Company Users', icon: 'CU', roles: ['PartnerCompany'] },
      { route: '/admin/roles', label: 'Roles', icon: 'RL', roles: ['Admin', 'SystemAdmin'] },
      { route: '/profile', label: 'Profile', icon: 'PR' }
    ];

    return all.filter((item) => !item.roles || this.authService.hasAnyRole(item.roles));
  });

  protected readonly filteredPolls = computed(() => {
    return this.polls().items;
  });

  protected readonly activePollCount = computed(() => this.pollStats().activePolls);
  protected readonly draftPollCount = computed(() => this.pollStats().draftPolls);
  protected readonly answersCount = computed(() => this.pollStats().answersCount);

  ngOnInit(): void {
    this.refreshData();
  }

  protected isAdmin(): boolean {
    return this.authService.hasAnyRole(['Admin', 'SystemAdmin', 'PartnerCompany']);
  }

  protected refreshData(): void {
    if (this.isAdmin()) {
      this.loadPolls();
      return;
    }
    this.loadCurrentPolls();
  }

  protected onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
  }

  protected onStatusChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.statusFilter.set((target.value as 'all' | 'active' | 'draft') ?? 'all');
    this.filters.pageNumber = 1;
    this.loadPolls();
  }

  protected applySearch(): void {
    this.filters.pageNumber = 1;
    this.filters.searchTerm = this.searchTerm().trim() || undefined;
    this.loadPolls();
  }

  protected toggleColumnsMenu(): void {
    this.showColumnsMenu.update((value) => !value);
  }

  protected isColumnVisible(key: PollColumnKey): boolean {
    return this.visibleColumns().includes(key);
  }

  protected toggleColumn(key: PollColumnKey): void {
    const current = this.visibleColumns();
    if (current.includes(key)) {
      if (current.length === 1) {
        return;
      }
      this.visibleColumns.set(current.filter((x) => x !== key));
      return;
    }
    this.visibleColumns.set([...current, key]);
  }

  protected changePage(page: number): void {
    if (page < 1 || page > (this.polls().totalPages || 1)) {
      return;
    }
    this.filters.pageNumber = page;
    this.loadPolls();
  }

  protected async deletePoll(id: string): Promise<void> {
    const accepted = await this.uiFeedback.confirm({
      title: 'Delete survey?',
      message: 'This action permanently removes the survey and cannot be undone.',
      confirmText: 'Delete',
      danger: true
    });

    if (!accepted) {
      return;
    }

    this.pollService.deletePoll(id).subscribe({
      next: () => {
        this.uiFeedback.success('Survey deleted', 'The selected survey was removed successfully.');
        this.loadPolls();
      },
      error: () => {
        this.uiFeedback.error('Delete failed', 'The survey could not be deleted. Please try again.');
      }
    });
  }

  private loadPolls(): void {
    this.pollService.getPolls(this.filters, this.statusFilter()).subscribe((result) => {
      this.polls.set(result.items);
      this.pollStats.set(result.stats);
    });
  }

  private loadCurrentPolls(): void {
    this.pollService.getCurrentPolls().subscribe((result) => this.availablePolls.set(result));
  }

}
