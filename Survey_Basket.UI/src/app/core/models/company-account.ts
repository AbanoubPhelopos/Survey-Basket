export interface CreateCompanyAccountRequest {
  companyName: string;
  contactEmail: string;
  firstName: string;
  lastName: string;
  websiteUrl?: string;
  linkedInUrl?: string;
  logoUrl?: string;
}

export interface CreateCompanyAccountResponse {
  companyId: string;
  companyAccountUserId: string;
  activationToken: string;
  activationState: 'PendingPassword' | 'Active' | 'Disabled';
}

export interface CompanyAccountListItemResponse {
  companyId: string;
  companyName: string;
  companyCode: string;
  companyIsActive: boolean;
  companyAccountUserId: string;
  accountFullName: string;
  contactEmail: string;
  isLocked: boolean;
  accountState: string;
  logoUrl: string;
  websiteUrl: string;
  linkedInUrl: string;
  createdOn: string;
}

export interface CompanyAccountsStatsResponse {
  totalCompanies: number;
  activeCompanies: number;
  inactiveCompanies: number;
  lockedAccounts: number;
}

export interface AdminCompanyUserListItemResponse {
  companyId: string;
  companyName: string;
  companyUserRecordId: string;
  displayName: string;
  businessIdentifier: string;
  isLocked: boolean;
  isPrimary: boolean;
}

export interface AdminCompanyUsersStatsResponse {
  totalUsers: number;
  lockedUsers: number;
  activeUsers: number;
  companiesCount: number;
}

export interface CompanyMagicLoginLinkResponse {
  companyAccountUserId: string;
  loginUrl: string;
  qrPayload: string;
  expiresOn: string;
}
