import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../core/services/account.service';
import {
  AdminCompanyUserListItemResponse,
  CompanyAccountListItemResponse,
  CreateCompanyAccountRequest,
  CreateCompanyAccountResponse
} from '../../core/models/company-account';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';
import { UserService } from '../../core/services/user.service';
import { RequestFilters } from '../../core/models/poll';
import { PagedList } from '../../core/models/service-result';

@Component({
  selector: 'app-companies',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './companies.component.html',
  styleUrls: ['./companies.component.scss']
})
export class CompaniesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly userService = inject(UserService);
  private readonly uiFeedback = inject(UiFeedbackService);

  readonly loading = signal(false);
  readonly showCreateModal = signal(false);
  readonly generatingToken = signal(false);
  readonly created = signal<CreateCompanyAccountResponse | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly accountFilters: RequestFilters = { pageNumber: 1, pageSize: 8, sortColumn: 'CreatedOn', sortDirection: 'DESC' };
  readonly userFilters: RequestFilters = { pageNumber: 1, pageSize: 8, sortColumn: 'CompanyName', sortDirection: 'ASC' };

  readonly companySearch = signal('');
  readonly companyState = signal<'all' | 'active' | 'locked' | 'inactive'>('all');
  readonly companyAccountsPage = signal<PagedList<CompanyAccountListItemResponse>>({
    items: [], pageNumber: 1, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false
  });
  readonly companyStats = signal({ totalCompanies: 0, activeCompanies: 0, inactiveCompanies: 0, lockedAccounts: 0 });

  readonly companyUsersSearch = signal('');
  readonly companyUsersStatus = signal<'all' | 'active' | 'locked'>('all');
  readonly selectedCompanyId = signal<string>('all');
  readonly adminCompanyUsersPage = signal<PagedList<AdminCompanyUserListItemResponse>>({
    items: [], pageNumber: 1, totalPages: 0, totalCount: 0, hasPreviousPage: false, hasNextPage: false
  });
  readonly adminCompanyUsersStats = signal({ totalUsers: 0, lockedUsers: 0, activeUsers: 0, companiesCount: 0 });

  readonly companyOptions = computed(() => {
    const unique = new Map<string, string>();
    this.companyAccountsPage().items.forEach((item) => {
      unique.set(item.companyId, item.companyName);
    });

    return Array.from(unique.entries()).map(([id, name]) => ({ id, name }));
  });

  readonly form = this.fb.group({
    companyName: ['', [Validators.required, Validators.minLength(2)]],
    contactEmail: ['', [Validators.required, Validators.email]],
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    websiteUrl: [''],
    linkedInUrl: [''],
    logoUrl: ['']
  });

  ngOnInit(): void {
    this.loadCompanyAccounts();
    this.loadAdminCompanyUsers();
  }

  createCompanyAccount(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.created.set(null);

    this.accountService.createCompanyAccount(this.form.getRawValue() as CreateCompanyAccountRequest).subscribe({
      next: (response) => {
        this.created.set(response);
        this.form.reset();
        this.successMessage.set('Company account created. Share the activation link and token securely.');
        this.uiFeedback.success('Company created', 'Company account created with a new activation token.');
        this.loading.set(false);
        this.showCreateModal.set(false);
        this.loadCompanyAccounts();
      },
      error: (error) => {
        if (error?.status === 403) {
          this.errorMessage.set('You are not authorized to create company accounts.');
        } else {
          this.errorMessage.set(error?.error?.detail || 'Company account creation failed. Please retry.');
        }
        this.uiFeedback.error('Company creation failed', this.errorMessage() || 'Unable to create company account.');
        this.loading.set(false);
      }
    });
  }

  openCreateModal(): void {
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  changeCompanyPage(page: number): void {
    if (page < 1 || page > (this.companyAccountsPage().totalPages || 1)) return;
    this.accountFilters.pageNumber = page;
    this.loadCompanyAccounts();
  }

  changeCompanyUsersPage(page: number): void {
    if (page < 1 || page > (this.adminCompanyUsersPage().totalPages || 1)) return;
    this.userFilters.pageNumber = page;
    this.loadAdminCompanyUsers();
  }

  onCompanySearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.companySearch.set(input.value);
    this.accountFilters.pageNumber = 1;
    this.loadCompanyAccounts();
  }

  onCompanyStateChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.companyState.set((select.value as 'all' | 'active' | 'locked' | 'inactive') ?? 'all');
    this.accountFilters.pageNumber = 1;
    this.loadCompanyAccounts();
  }

  onCompanyUsersSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.companyUsersSearch.set(input.value);
    this.userFilters.pageNumber = 1;
    this.loadAdminCompanyUsers();
  }

  onCompanyUsersStatusChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.companyUsersStatus.set((select.value as 'all' | 'active' | 'locked') ?? 'all');
    this.userFilters.pageNumber = 1;
    this.loadAdminCompanyUsers();
  }

  onSelectedCompanyChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedCompanyId.set(select.value || 'all');
    this.userFilters.pageNumber = 1;
    this.loadAdminCompanyUsers();
  }

  setCompanyAccountLockState(account: CompanyAccountListItemResponse, locked: boolean): void {
    this.userService.setCompanyAccountLockState(account.companyAccountUserId, locked).subscribe({
      next: () => {
        this.uiFeedback.success(locked ? 'Account locked' : 'Account unlocked', `${account.companyName} account state updated.`);
        this.loadCompanyAccounts();
      },
      error: () => {
        this.uiFeedback.error('Update failed', 'Unable to update company account lock state.');
      }
    });
  }

  getActivationPath(companyAccountUserId: string): string {
    return `/activate-company/${companyAccountUserId}`;
  }

  getActivationUrl(companyAccountUserId: string): string {
    if (typeof window === 'undefined') {
      return this.getActivationPath(companyAccountUserId);
    }

    return `${window.location.origin}${this.getActivationPath(companyAccountUserId)}`;
  }

  openActivationPage(companyAccountUserId: string): void {
    if (typeof window !== 'undefined') {
      window.open(this.getActivationUrl(companyAccountUserId), '_blank', 'noopener,noreferrer');
    }
  }

  copyActivationToken(): void {
    const token = this.created()?.activationToken;
    if (!token) return;
    this.copyText(token, 'Activation token copied.');
  }

  copyActivationLink(): void {
    const companyAccountUserId = this.created()?.companyAccountUserId;
    if (!companyAccountUserId) return;
    this.copyText(this.getActivationUrl(companyAccountUserId), 'Activation link copied.');
  }

  regenerateToken(): void {
    const companyAccountUserId = this.created()?.companyAccountUserId;
    if (!companyAccountUserId) return;

    this.generatingToken.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.accountService.generateCompanyActivationToken(companyAccountUserId).subscribe({
      next: (result) => {
        const current = this.created();
        if (!current) {
          this.generatingToken.set(false);
          return;
        }

        this.created.set({ ...current, activationToken: result.activationToken });
        this.successMessage.set('A new activation token was generated.');
        this.uiFeedback.success('Token regenerated', 'A new activation token has been issued.');
        this.generatingToken.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error?.error?.detail || 'Failed to regenerate activation token.');
        this.uiFeedback.error('Token generation failed', this.errorMessage() || 'Failed to regenerate token.');
        this.generatingToken.set(false);
      }
    });
  }

  private loadCompanyAccounts(): void {
    this.accountFilters.searchTerm = this.companySearch().trim() || undefined;
    this.userService.getCompanyAccounts(this.accountFilters, this.companyState()).subscribe({
      next: (result) => {
        this.companyAccountsPage.set(result.items);
        this.companyStats.set(result.stats);
      },
      error: () => {
        this.uiFeedback.error('Load failed', 'Unable to load company accounts.');
      }
    });
  }

  private loadAdminCompanyUsers(): void {
    this.userFilters.searchTerm = this.companyUsersSearch().trim() || undefined;
    const companyId = this.selectedCompanyId() === 'all' ? undefined : this.selectedCompanyId();

    this.userService.getAdminCompanyUsers(this.userFilters, companyId, this.companyUsersStatus()).subscribe({
      next: (result) => {
        this.adminCompanyUsersPage.set(result.items);
        this.adminCompanyUsersStats.set(result.stats);
      },
      error: () => {
        this.uiFeedback.error('Load failed', 'Unable to load company users list.');
      }
    });
  }

  private copyText(value: string, successMessage: string): void {
    if (typeof navigator !== 'undefined' && navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(value)
        .then(() => this.successMessage.set(successMessage))
        .then(() => this.uiFeedback.success('Copied', successMessage))
        .catch(() => {
          this.errorMessage.set('Clipboard copy failed. Please copy manually.');
          this.uiFeedback.error('Copy failed', 'Clipboard copy failed. Please copy manually.');
        });
      return;
    }

    this.errorMessage.set('Clipboard API is unavailable. Please copy manually.');
    this.uiFeedback.error('Clipboard unavailable', 'Clipboard API is unavailable. Please copy manually.');
  }
}
