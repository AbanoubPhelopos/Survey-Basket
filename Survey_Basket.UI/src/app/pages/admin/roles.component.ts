import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RoleService } from '../../core/services/role.service';
import { RoleResponse } from '../../core/models/role';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="roles-page">
      <header class="sb-surface sb-hero-gradient roles-header">
        <h1>Roles Management</h1>
        <p class="sb-muted">View available roles and permission sets.</p>
      </header>

      <section class="sb-surface roles-list">
        @if (loading()) {
        <p>Loading roles...</p>
        }

        @if (!loading() && roles().length === 0) {
        <p class="sb-muted">No roles available.</p>
        }

        @for (role of roles(); track role.id) {
        <article class="role-card">
          <div>
            <h2>{{ role.name }}</h2>
            <p class="sb-muted">{{ role.permissions.length }} permissions</p>
          </div>
          <span class="role-status" [class.role-status--inactive]="role.isDisabled">
            {{ role.isDisabled ? 'Disabled' : 'Active' }}
          </span>
        </article>
        }
      </section>
    </section>
  `,
  styles: [
    `
      .roles-page { display: grid; gap: 1rem; }
      .roles-header, .roles-list { padding: 1rem; }
      .role-card {
        border: 1px solid var(--border);
        border-radius: 0.8rem;
        padding: 0.75rem;
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.7rem;
      }
      .role-card h2 { margin: 0; font-size: 1rem; }
      .role-card p { margin: 0.2rem 0 0; }
      .role-status {
        border: 1px solid var(--border);
        border-radius: 999px;
        padding: 0.2rem 0.6rem;
        font-size: 0.75rem;
      }
      .role-status--inactive { opacity: 0.65; }
    `
  ]
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
