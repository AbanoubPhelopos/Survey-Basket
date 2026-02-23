import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RequestFilters } from '../../core/models/poll';
import { PagedList } from '../../core/models/service-result';
import { UserService } from '../../core/services/user.service';
import { CompanyUserInviteResponse, CreateCompanyUserInviteRequest, CreateCompanyUserRecordRequest, CreateCompanyUserRecordResponse } from '../../core/models/company-user-record';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-company-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './company-users.component.html',
  styleUrls: ['./company-users.component.scss']
})
export class CompanyUsersComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly uiFeedback = inject(UiFeedbackService);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly recordsPage = signal<PagedList<CreateCompanyUserRecordResponse>>({ items: [], pageNumber: 1, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false });
  readonly searchTerm = signal('');
  readonly idFilter = signal<'all' | 'short' | 'long'>('all');
  readonly visibleColumns = signal<Array<'name' | 'identifier' | 'recordId'>>(['name', 'identifier', 'recordId']);
  readonly showColumnsMenu = signal(false);
  readonly stats = signal({ totalRecords: 0, shortIdentifiers: 0, longIdentifiers: 0 });
  readonly invites = signal<CompanyUserInviteResponse[]>([]);
  readonly inviteUrl = signal('');
  readonly totalPages = computed(() => this.recordsPage().totalPages || 1);
  readonly pagedRecords = computed(() => this.recordsPage().items);
  readonly recordsCount = computed(() => this.stats().totalRecords);
  readonly shortIdentifierCount = computed(() => this.stats().shortIdentifiers);
  readonly longIdentifierCount = computed(() => this.stats().longIdentifiers);

  readonly filters: RequestFilters = { pageNumber: 1, pageSize: 6, sortColumn: 'DisplayName', sortDirection: 'ASC' };

  readonly form = this.fb.group({
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    businessIdentifier: ['', [Validators.required, Validators.minLength(3)]]
  });

  readonly inviteForm = this.fb.group({
    email: [''],
    mobile: [''],
    expiresInMinutes: [15]
  });

  createRecord(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.userService.createCompanyUserRecord(this.form.getRawValue() as CreateCompanyUserRecordRequest).subscribe({
      next: (response) => {
        this.form.reset();
        this.filters.pageNumber = 1;
        this.loadRecords();
        this.uiFeedback.success('Record created', 'Company user record was created successfully.');
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        if (error?.status === 403) {
          this.errorMessage.set('Only company account can create records for its own company.');
        } else {
          this.errorMessage.set(error?.error?.detail || 'Failed to create company user record.');
        }
        this.uiFeedback.error('Create failed', this.errorMessage() || 'Failed to create company user record.');
      }
    });
  }

  ngOnInit(): void {
    this.loadRecords();
    this.loadInvites();
  }

  createInvite(): void {
    const payload = this.inviteForm.getRawValue() as CreateCompanyUserInviteRequest;
    if (!payload.email && !payload.mobile) {
      this.uiFeedback.warning('Identity required', 'Provide email or mobile for secure invite generation.');
      return;
    }

    this.userService.createCompanyUserInvite(payload).subscribe({
      next: (invite) => {
        const absolute = typeof window === 'undefined' ? invite.inviteUrl : `${window.location.origin}${invite.inviteUrl}`;
        this.inviteUrl.set(absolute);
        this.uiFeedback.success('Invite created', 'One-time QR invite generated successfully.');
        this.loadInvites();
      },
      error: () => this.uiFeedback.error('Invite failed', 'Unable to create company user invite.')
    });
  }

  qrImageUrl(payload: string): string {
    return `https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=${encodeURIComponent(payload)}`;
  }

  onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
    this.filters.pageNumber = 1;
    this.loadRecords();
  }

  onFilterChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.idFilter.set((target.value as 'all' | 'short' | 'long') ?? 'all');
    this.filters.pageNumber = 1;
    this.loadRecords();
  }

  isColumnVisible(column: 'name' | 'identifier' | 'recordId'): boolean {
    return this.visibleColumns().includes(column);
  }

  toggleColumn(column: 'name' | 'identifier' | 'recordId'): void {
    const current = this.visibleColumns();
    if (current.includes(column)) {
      if (current.length === 1) {
        return;
      }
      this.visibleColumns.set(current.filter((x) => x !== column));
      return;
    }

    this.visibleColumns.set([...current, column]);
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }

    this.filters.pageNumber = page;
    this.loadRecords();
  }

  private loadRecords(): void {
    this.filters.searchTerm = this.searchTerm().trim() || undefined;
    this.userService.getCompanyUserRecords(this.filters, this.idFilter()).subscribe((result) => {
      this.recordsPage.set(result.items);
      this.stats.set(result.stats);
    });
  }

  private loadInvites(): void {
    this.userService.getCompanyUserInvites().subscribe({
      next: (items) => this.invites.set(items),
      error: () => this.invites.set([])
    });
  }
}
