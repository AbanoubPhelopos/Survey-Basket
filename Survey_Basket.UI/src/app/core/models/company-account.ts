export interface CreateCompanyAccountRequest {
  companyName: string;
  contactEmail: string;
  firstName: string;
  lastName: string;
}

export interface CreateCompanyAccountResponse {
  companyId: string;
  companyAccountUserId: string;
  activationToken: string;
  activationState: 'PendingPassword' | 'Active' | 'Disabled';
}
