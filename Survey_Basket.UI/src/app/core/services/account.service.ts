import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL, COMPANY_ACCOUNT_ENDPOINTS } from '../constants/api.constants';
import { UserProfileResponse, UpdateProfileRequest, ChangePasswordRequest } from '../models/account';
import { CreateCompanyAccountRequest, CreateCompanyAccountResponse } from '../models/company-account';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private apiUrl = `${API_BASE_URL.replace('/api', '')}/me`;
  private usersApiUrl = `${API_BASE_URL}/users`;
  
  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfileResponse> {
    return this.http.get<UserProfileResponse>(this.apiUrl);
  }

  updateProfile(request: UpdateProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/info`, request);
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/change-password`, request);
  }

  createCompanyAccount(request: CreateCompanyAccountRequest): Observable<CreateCompanyAccountResponse> {
    return this.http.post<CreateCompanyAccountResponse>(`${this.usersApiUrl}/company-accounts`, request);
  }

  generateCompanyActivationToken(companyAccountUserId: string): Observable<{ activationToken: string }> {
    return this.http.post<{ activationToken: string }>(
      `${this.apiUrl}/company-accounts/${companyAccountUserId}/activation-token`,
      {}
    );
  }

  getCompanyUsersEndpoint(companyId: string): string {
    return COMPANY_ACCOUNT_ENDPOINTS.users(companyId);
  }
}
