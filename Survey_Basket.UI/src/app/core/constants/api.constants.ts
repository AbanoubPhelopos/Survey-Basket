export const API_BASE_URL = 'http://localhost:5002/api';

export const COMPANY_ACCOUNT_ENDPOINTS = {
  base: `${API_BASE_URL}/companies`,
  activate: (companyId: string) => `${API_BASE_URL}/companies/${companyId}/activate`,
  users: (companyId: string) => `${API_BASE_URL}/companies/${companyId}/users`
} as const;
