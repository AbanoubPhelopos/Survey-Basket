import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RequestFilters } from '../../core/models/poll';
import { UserResponse } from '../../core/models/user';
import { PagedList } from '../../core/models/service-result';
import { UserService } from '../../core/services/user.service';

type UserColumnKey = 'user' | 'email' | 'roles' | 'status';

interface UserColumn {
  key: UserColumnKey;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-wrapper pt-4">
      <div class="sb-stats-grid">
        <article class="sb-stat"><p class="sb-stat__label">Total Users</p><p class="sb-stat__value">{{ stats().totalUsers }}</p></article>
        <article class="sb-stat"><p class="sb-stat__label">Active Users</p><p class="sb-stat__value">{{ stats().activeUsers }}</p></article>
        <article class="sb-stat"><p class="sb-stat__label">Disabled Users</p><p class="sb-stat__value">{{ stats().disabledUsers }}</p></article>
        <article class="sb-stat"><p class="sb-stat__label">Distinct Roles</p><p class="sb-stat__value">{{ stats().distinctRoles }}</p></article>
      </div>

      <div class="sb-toolbar">
        <div class="sb-toolbar__group">
          <input class="sb-input sb-search" [value]="searchTerm()" (input)="onSearchInput($event)" placeholder="Search by name or email" />
          <select class="sb-select" [value]="statusFilter()" (change)="onStatusChange($event)">
            <option value="all">All statuses</option>
            <option value="active">Active only</option>
            <option value="disabled">Disabled only</option>
          </select>
        </div>
      </div>

      <div class="sb-table-wrap">
        <div class="overflow-x-auto">
          <table class="sb-table">
            <thead><tr><th>User</th><th>Email Address</th><th>Roles</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody>
              @for (user of usersPage().items; track user.id) {
                <tr>
                  <td class="font-semibold">{{ user.firstName }} {{ user.lastName }}</td>
                  <td class="text-[0.82rem] font-mono text-[var(--text-soft)]">{{ user.email }}</td>
                  <td>
                    <div class="flex flex-wrap gap-1.5">
                      @for (role of user.roles; track role) {
                        <span class="inline-flex items-center px-2 py-0.5 rounded text-[0.65rem] font-bold uppercase tracking-wider bg-[var(--bg-soft)] border border-[var(--border)]">{{ role }}</span>
                      }
                    </div>
                  </td>
                  <td>
                    <span class="inline-flex items-center px-2.5 py-1 rounded text-[0.65rem] font-bold uppercase tracking-wider border" [ngClass]="user.isDisabled ? 'bg-red-50 text-red-700 border-red-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'">{{ user.isDisabled ? 'Disabled' : 'Active' }}</span>
                  </td>
                  <td><button class="sb-btn-secondary text-xs px-2.5 py-1.5">Manage</button></td>
                </tr>
              }
              @if (!usersPage().items.length) { <tr><td colspan="5"><div class="sb-empty text-sm my-3">No users found for the selected filter.</div></td></tr> }
            </tbody>
          </table>
        </div>

        @if (usersPage().totalPages > 1) {
          <div class="sb-pagination">
            <p class="sb-pagination__info">Page {{ usersPage().pageNumber }} of {{ usersPage().totalPages }}</p>
            <div class="sb-pagination__controls">
              <button class="sb-pagination__button" [disabled]="!usersPage().hasPreviousPage" (click)="changePage(usersPage().pageNumber - 1)">Prev</button>
              <button class="sb-pagination__button" [disabled]="!usersPage().hasNextPage" (click)="changePage(usersPage().pageNumber + 1)">Next</button>
            </div>
          </div>
        }
      </div>
    </section>
  `
})
export class UsersComponent implements OnInit {
  protected readonly allColumns: UserColumn[] = [
    { key: 'user', label: 'User', icon: 'U' },
    { key: 'email', label: 'Email', icon: 'E' },
    { key: 'roles', label: 'Roles', icon: 'R' },
    { key: 'status', label: 'Status', icon: 'S' }
  ];

  protected readonly usersPage = signal<PagedList<UserResponse>>({ items: [], pageNumber: 1, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false });
  protected readonly searchTerm = signal('');
  protected readonly statusFilter = signal<'all' | 'active' | 'disabled'>('all');
  protected readonly stats = signal({ totalUsers: 0, activeUsers: 0, disabledUsers: 0, distinctRoles: 0 });
  protected readonly filters: RequestFilters = { pageNumber: 1, pageSize: 8, sortColumn: 'FirstName', sortDirection: 'ASC' };

  private readonly userService = inject(UserService);

  ngOnInit(): void {
    this.loadUsers();
  }

  protected onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
    this.filters.pageNumber = 1;
    this.loadUsers();
  }

  protected onStatusChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.statusFilter.set((target.value as 'all' | 'active' | 'disabled') ?? 'all');
    this.filters.pageNumber = 1;
    this.loadUsers();
  }

  protected changePage(page: number): void {
    if (page < 1) return;
    this.filters.pageNumber = page;
    this.loadUsers();
  }

  private loadUsers(): void {
    this.filters.searchTerm = this.searchTerm().trim() || undefined;
    this.userService.getUsers(this.filters, this.statusFilter()).subscribe((res) => {
      this.usersPage.set(res.items);
      this.stats.set(res.stats);
    });
  }
}
