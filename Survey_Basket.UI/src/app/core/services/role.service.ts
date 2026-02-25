import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { RoleResponse, RoleStatsResponse } from '../models/role';
import { ServiceListResult, ServiceResult } from '../models/service-result';
import { RequestFilters } from '../models/poll';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly apiUrl = `${API_BASE_URL}/roles`;

  constructor(private readonly http: HttpClient) {}

  getRoles(filters: RequestFilters, status = 'all', includeDisabled = false): Observable<ServiceListResult<RoleResponse, RoleStatsResponse>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString())
      .set('status', status)
      .set('includeDisabled', includeDisabled.toString());

    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.sortColumn) params = params.set('sortColumn', filters.sortColumn);
    if (filters.sortDirection) params = params.set('sortDirection', filters.sortDirection);

    return this.http.get<ServiceResult<ServiceListResult<RoleResponse, RoleStatsResponse>>>(this.apiUrl, { params }).pipe(
      map((result) => {
        if (!result.succeeded || !result.data) {
          throw new Error(result.error?.description ?? 'Failed to load roles');
        }
        return result.data;
      })
    );
  }

  getRoleStats(): Observable<RoleStatsResponse> {
    return this.http.get<RoleStatsResponse>(`${this.apiUrl}/stats`);
  }
}
