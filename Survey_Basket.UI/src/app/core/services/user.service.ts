import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { UserResponse, UsersStatsResponse } from '../models/user';
import { CompanyUserRecordsStatsResponse, CreateCompanyUserRecordRequest, CreateCompanyUserRecordResponse } from '../models/company-user-record';
import { ServiceListResult, ServiceResult } from '../models/service-result';
import { RequestFilters } from '../models/poll';

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
}
