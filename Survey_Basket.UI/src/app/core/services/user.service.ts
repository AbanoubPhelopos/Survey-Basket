import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { UserResponse, UsersStatsResponse } from '../models/user';
import { CompanyUserInviteResponse, CompanyUserRecordsStatsResponse, CreateCompanyUserInviteRequest, CreateCompanyUserRecordRequest, CreateCompanyUserRecordResponse } from '../models/company-user-record';
import { ServiceListResult, ServiceResult } from '../models/service-result';
import { RequestFilters } from '../models/poll';
import { AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse, CompanyAccountListItemResponse, CompanyAccountsStatsResponse, CompanyMagicLoginLinkResponse } from '../models/company-account';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${API_BASE_URL}/users`;

  constructor(private http: HttpClient) {}

  getUsers(filters: RequestFilters, status = 'all'): Observable<ServiceListResult<UserResponse, UsersStatsResponse>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString())
      .set('status', status);

    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortColumn) params = params.set('sortColumn', filters.sortColumn);
    if (filters.sortDirection) params = params.set('sortDirection', filters.sortDirection);

    return this.http.get<ServiceResult<ServiceListResult<UserResponse, UsersStatsResponse>>>(this.apiUrl, { params }).pipe(
      map((result) => {
        if (!result.succeeded || !result.data) {
          throw new Error(result.error?.description ?? 'Failed to load users');
        }
        return result.data;
      })
    );
  }

  getUsersStats(): Observable<UsersStatsResponse> {
    return this.http.get<UsersStatsResponse>(`${this.apiUrl}/stats`);
  }

  createCompanyUserRecord(request: CreateCompanyUserRecordRequest): Observable<CreateCompanyUserRecordResponse> {
    return this.http.post<CreateCompanyUserRecordResponse>(`${this.apiUrl}/company-user-records`, request);
  }

  getCompanyUserRecords(filters: RequestFilters, identifierMode = 'all'): Observable<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString())
      .set('identifierMode', identifierMode);

    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);

    return this.http.get<ServiceResult<ServiceListResult<CreateCompanyUserRecordResponse, CompanyUserRecordsStatsResponse>>>(`${this.apiUrl}/company-user-records`, { params }).pipe(
      map((result) => {
        if (!result.succeeded || !result.data) {
          throw new Error(result.error?.description ?? 'Failed to load company user records');
        }
        return result.data;
      })
    );
  }

  getCompanyUserRecordStats(): Observable<CompanyUserRecordsStatsResponse> {
    return this.http.get<CompanyUserRecordsStatsResponse>(`${this.apiUrl}/company-user-records/stats`);
  }

  getCompanyAccounts(filters: RequestFilters, state = 'all'): Observable<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString())
      .set('state', state);

    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortColumn) params = params.set('sortColumn', filters.sortColumn);
    if (filters.sortDirection) params = params.set('sortDirection', filters.sortDirection);

    return this.http.get<ServiceResult<ServiceListResult<CompanyAccountListItemResponse, CompanyAccountsStatsResponse>>>(`${this.apiUrl}/company-accounts`, { params }).pipe(
      map((result) => {
        if (!result.succeeded || !result.data) {
          throw new Error(result.error?.description ?? 'Failed to load company accounts');
        }
        return result.data;
      })
    );
  }

  setCompanyAccountLockState(companyAccountUserId: string, locked: boolean): Observable<void> {
    const params = new HttpParams().set('locked', String(locked));
    return this.http.put<void>(`${this.apiUrl}/company-accounts/${companyAccountUserId}/lock-state`, null, { params });
  }

  generateCompanyMagicLoginLink(companyAccountUserId: string): Observable<CompanyMagicLoginLinkResponse> {
    return this.http.post<CompanyMagicLoginLinkResponse>(`${this.apiUrl}/company-accounts/${companyAccountUserId}/magic-login-link`, {});
  }

  getAdminCompanyUsers(filters: RequestFilters, companyId?: string, status = 'all'): Observable<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString())
      .set('status', status);

    if (companyId) params = params.set('companyId', companyId);
    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortColumn) params = params.set('sortColumn', filters.sortColumn);
    if (filters.sortDirection) params = params.set('sortDirection', filters.sortDirection);

    return this.http.get<ServiceResult<ServiceListResult<AdminCompanyUserListItemResponse, AdminCompanyUsersStatsResponse>>>(`${this.apiUrl}/admin/company-users`, { params }).pipe(
      map((result) => {
        if (!result.succeeded || !result.data) {
          throw new Error(result.error?.description ?? 'Failed to load company users');
        }
        return result.data;
      })
    );
  }

  createCompanyUserInvite(request: CreateCompanyUserInviteRequest): Observable<CompanyUserInviteResponse> {
    return this.http.post<CompanyUserInviteResponse>(`${this.apiUrl}/company-user-invites`, request);
  }

  getCompanyUserInvites(): Observable<CompanyUserInviteResponse[]> {
    return this.http.get<CompanyUserInviteResponse[]>(`${this.apiUrl}/company-user-invites`);
  }
}
