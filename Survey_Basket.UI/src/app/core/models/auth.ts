export interface LoginRequest {
  email: string;
  password: string;
}

export type AppRole = 'Admin' | 'SystemAdmin' | 'PartnerCompany' | 'CompanyUser' | 'Member';

export interface CapabilityContext {
  roles: AppRole[];
  permissions: string[];
  accountType?: 'AdminAccount' | 'CompanyAccount';
  requiresActivation?: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  companyId?: string;
  isCompanyAccount?: boolean;
}

export interface ActivateCompanyAccountRequest {
  activationToken: string;
  newPassword: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  refreshTokenExpiration: Date;
  expiresIn: number;
  roles: string[];
  permissions: string[];
  firstName: string;
  lastName: string;
  email: string;
  userId: string;
  accountType?: 'AdminAccount' | 'CompanyAccount';
  requiresActivation?: boolean;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  permissions: string[];
  roles: string[];
  accountType?: 'AdminAccount' | 'CompanyAccount';
  requiresActivation?: boolean;
}
