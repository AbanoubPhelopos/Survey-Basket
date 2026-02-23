import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RoleService } from '../../core/services/role.service';
import { RoleResponse } from '../../core/models/role';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-wrapper pt-4">
      <header class="page-header">
        <p class="text-xs tracking-wider text-[var(--accent)] font-bold mb-1 uppercase">Admin Control</p>
        <h1 class="page-header__title">Roles Management</h1>
        <p class="page-header__desc">View available roles and permission sets in the system.</p>
      </header>

      <div class="sb-surface rounded-xl border border-[var(--border)] overflow-hidden">
        @if (loading()) {
        <div class="py-12 flex justify-center">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--accent)]"></div>
        </div>
        }

        @if (!loading() && roles().length === 0) {
        <div class="sb-empty m-6 text-sm">
          No roles available.
        </div>
        }

        @if (!loading() && roles().length > 0) {
        <ul class="divide-y divide-[var(--border)]">
          @for (role of roles(); track role.id) {
          <li class="p-6 flex items-center justify-between hover:bg-[var(--sidebar-hover)] transition-colors group">
            <div class="flex-1">
              <h2 class="text-[1.05rem] font-bold text-[var(--text)] group-hover:text-[var(--accent)] transition-colors mb-1">{{ role.name }}</h2>
              <p class="text-[0.85rem] text-[var(--text-soft)] font-medium flex items-center gap-2">
                 <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-4 h-4"><path fill-rule="evenodd" d="M10 1a4.5 4.5 0 00-4.5 4.5V9H5a2 2 0 00-2 2v6a2 2 0 002 2h10a2 2 0 002-2v-6a2 2 0 00-2-2h-.5V5.5A4.5 4.5 0 0010 1zm3 8V5.5a3 3 0 10-6 0V9h6z" clip-rule="evenodd" /></svg>
                 {{ role.permissions.length }} scoped permissions attached
              </p>
            </div>
            <div>
              <span class="inline-flex items-center px-2.5 py-1 rounded text-[0.7rem] font-bold uppercase tracking-wider border shadow-sm"
                    [ngClass]="role.isDisabled ? 'bg-red-50 text-red-700 border-red-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'">
                {{ role.isDisabled ? 'Disabled' : 'Active' }}
              </span>
            </div>
          </li>
          }
        </ul>
        }
      </div>
    </section>
  `
})
export class RolesComponent implements OnInit {
  private readonly roleService = inject(RoleService);
  readonly roles = signal<RoleResponse[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.roleService.getRoles(true).subscribe({
      next: (res) => {
        this.roles.set(res);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
