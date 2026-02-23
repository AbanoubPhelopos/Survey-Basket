import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../core/services/user.service';
import { UserResponse } from '../../core/models/user';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-wrapper pt-4">
      <header class="page-header">
        <p class="text-xs tracking-wider text-[var(--accent)] font-bold mb-1 uppercase">Admin Control</p>
        <h1 class="page-header__title">Manage Users</h1>
        <p class="page-header__desc">View and monitor authorized system users and their roles.</p>
      </header>

      <div class="sb-surface rounded-xl border border-[var(--border)] overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm text-[var(--text)]">
            <thead class="bg-[var(--bg-soft)] text-[0.75rem] font-bold uppercase text-[var(--text-soft)] border-b border-[var(--border)]">
              <tr>
                <th scope="col" class="px-6 py-3 tracking-wider">User</th>
                <th scope="col" class="px-6 py-3 tracking-wider">Email Address</th>
                <th scope="col" class="px-6 py-3 tracking-wider">Roles</th>
                <th scope="col" class="px-6 py-3 tracking-wider">Status</th>
                <th scope="col" class="px-6 py-3 tracking-wider text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-[var(--border)]">
              <tr *ngFor="let user of users()" class="hover:bg-[var(--sidebar-hover)] transition-colors group">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center gap-3">
                    <div class="h-9 w-9 rounded-full bg-[var(--sidebar-avatar-bg)] border border-[var(--border)] flex items-center justify-center text-[var(--text-soft)] text-xs font-bold shadow-sm group-hover:bg-[var(--accent)] group-hover:text-white transition-colors">
                      {{ user.firstName[0] }}{{ user.lastName[0] }}
                    </div>
                    <div>
                      <div class="text-[0.95rem] font-bold text-[var(--text)] leading-tight">{{ user.firstName }} {{ user.lastName }}</div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-[0.85rem] font-mono text-[var(--text-soft)]">{{ user.email }}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex flex-wrap gap-1.5">
                     <span *ngFor="let role of user.roles" class="inline-flex items-center px-2 py-0.5 rounded text-[0.65rem] font-bold uppercase tracking-wider bg-[var(--bg-soft)] border border-[var(--border)] text-[var(--text)] shadow-sm">
                       {{ role }}
                     </span>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="inline-flex items-center px-2.5 py-1 rounded text-[0.65rem] font-bold uppercase tracking-wider border shadow-sm" 
                        [ngClass]="user.isDisabled ? 'bg-red-50 text-red-700 border-red-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'">
                    {{ user.isDisabled ? 'Disabled' : 'Active' }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
                  <button class="text-[var(--text-soft)] hover:text-[var(--accent)] font-semibold transition-colors px-2 py-1 bg-[var(--bg-soft)] rounded border border-[var(--border)] text-xs shadow-sm">
                    Manage
                  </button>
                </td>
              </tr>
              <tr *ngIf="users().length === 0">
                <td colspan="5" class="px-6 py-12">
                   <div class="sb-empty text-sm">No users found.</div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>
  `
})
export class UsersComponent implements OnInit {
  users = signal<UserResponse[]>([]);
  private userService = inject(UserService);

  ngOnInit() {
    this.userService.getUsers().subscribe(res => this.users.set(res));
  }
}
