import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RequestFilters } from '../../core/models/poll';
import { RoleResponse } from '../../core/models/role';
import { PagedList } from '../../core/models/service-result';
import { RoleService } from '../../core/services/role.service';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-wrapper pt-4">
      <div class="sb-stats-grid">
        <article class="sb-stat"><p class="sb-stat__label">Total Roles</p><p class="sb-stat__value">{{ stats().totalRoles }}</p></article>
        <article class="sb-stat"><p class="sb-stat__label">Active Roles</p><p class="sb-stat__value">{{ stats().activeRoles }}</p></article>
        <article class="sb-stat"><p class="sb-stat__label">Disabled Roles</p><p class="sb-stat__value">{{ stats().disabledRoles }}</p></article>
        <article class="sb-stat"><p class="sb-stat__label">Permission Links</p><p class="sb-stat__value">{{ stats().permissionLinks }}</p></article>
      </div>

      <div class="sb-toolbar">
        <div class="sb-toolbar__group">
          <input class="sb-input sb-search" [value]="searchTerm()" (input)="onSearchInput($event)" placeholder="Search by role name" />
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
            <thead><tr><th>Role</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody>
              @for (role of rolesPage().items; track role.id) {
                <tr>
                  <td class="font-semibold">{{ role.name }}</td>
                  <td>
                    <span class="inline-flex items-center px-2.5 py-1 rounded text-[0.65rem] font-bold uppercase tracking-wider border" [ngClass]="role.isDeleted ? 'bg-red-50 text-red-700 border-red-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'">{{ role.isDeleted ? 'Disabled' : 'Active' }}</span>
                  </td>
                  <td><button class="sb-btn-secondary text-xs px-2.5 py-1.5">Manage</button></td>
                </tr>
              }
              @if (!rolesPage().items.length) { <tr><td colspan="3"><div class="sb-empty text-sm my-3">No roles match your filters.</div></td></tr> }
            </tbody>
          </table>
        </div>

        @if (rolesPage().totalPages > 1) {
          <div class="sb-pagination">
            <p class="sb-pagination__info">Page {{ rolesPage().pageNumber }} of {{ rolesPage().totalPages }}</p>
            <div class="sb-pagination__controls">
              <button class="sb-pagination__button" [disabled]="!rolesPage().hasPreviousPage" (click)="changePage(rolesPage().pageNumber - 1)">Prev</button>
              <button class="sb-pagination__button" [disabled]="!rolesPage().hasNextPage" (click)="changePage(rolesPage().pageNumber + 1)">Next</button>
            </div>
          </div>
        }
      </div>
    </section>
  `
})
export class RolesComponent implements OnInit {
  protected readonly rolesPage = signal<PagedList<RoleResponse>>({ items: [], pageNumber: 1, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false });
  protected readonly searchTerm = signal('');
  protected readonly statusFilter = signal<'all' | 'active' | 'disabled'>('all');
  protected readonly stats = signal({ totalRoles: 0, activeRoles: 0, disabledRoles: 0, permissionLinks: 0 });
  protected readonly filters: RequestFilters = { pageNumber: 1, pageSize: 8, sortColumn: 'Name', sortDirection: 'ASC' };

  private readonly roleService = inject(RoleService);

  ngOnInit(): void {
    this.loadRoles();
  }

  protected onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
    this.filters.pageNumber = 1;
    this.loadRoles();
  }

  protected onStatusChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.statusFilter.set((target.value as 'all' | 'active' | 'disabled') ?? 'all');
    this.filters.pageNumber = 1;
    this.loadRoles();
  }

  protected changePage(page: number): void {
    if (page < 1) return;
    this.filters.pageNumber = page;
    this.loadRoles();
  }

  private loadRoles(): void {
    this.filters.searchTerm = this.searchTerm().trim() || undefined;
    this.roleService.getRoles(this.filters, this.statusFilter(), true).subscribe((res) => {
      this.rolesPage.set(res.items);
      this.stats.set(res.stats);
    });
  }
}
