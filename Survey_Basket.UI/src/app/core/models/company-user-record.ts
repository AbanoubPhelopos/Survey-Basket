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

export interface CompanyUserRecordsStatsResponse {
  totalRecords: number;
  shortIdentifiers: number;
  longIdentifiers: number;
}

export interface CreateCompanyUserInviteRequest {
  email?: string;
  mobile?: string;
  expiresInMinutes?: number;
}

export interface CompanyUserInviteResponse {
  inviteId: string;
  companyId: string;
  inviteUrl: string;
  qrPayload: string;
  expiresOn: string;
  emailHint?: string;
  mobileHint?: string;
  isUsed: boolean;
}

export interface CreateCompanyPollAccessLinkRequest {
  pollId: string;
  expiresInMinutes?: number;
}

export interface CompanyPollAccessLinkResponse {
  linkId: string;
  companyId: string;
  pollId: string;
  joinUrl: string;
  qrPayload: string;
  expiresOn: string;
}
