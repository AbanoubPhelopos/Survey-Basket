export interface CreateCompanyUserRecordRequest {
  displayName: string;
  businessIdentifier: string;
}

export interface CreateCompanyUserRecordResponse {
  companyUserRecordId: string;
  companyId: string;
  displayName: string;
  businessIdentifier: string;
  authenticated: false;
}
