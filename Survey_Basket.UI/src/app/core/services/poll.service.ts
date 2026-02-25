import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { Poll, PollResponse, CreatePollRequest, UpdatePollRequest, RequestFilters, PollStatsResponse } from '../models/poll';
import { API_BASE_URL } from '../constants/api.constants';
import { ServiceListResult, ServiceResult } from '../models/service-result';

@Injectable({
  providedIn: 'root'
})
export class PollService {
  private apiUrl = `${API_BASE_URL}/polls`;

  constructor(private http: HttpClient) {}

  getPolls(filters: RequestFilters, status = 'all'): Observable<ServiceListResult<PollResponse, PollStatsResponse>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString());

    if (filters.sortColumn) params = params.set('sortColumn', filters.sortColumn);
    if (filters.sortDirection) params = params.set('sortDirection', filters.sortDirection);
    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    params = params.set('status', status);

    return this.http.get<ServiceResult<ServiceListResult<PollResponse, PollStatsResponse>>>(this.apiUrl, { params }).pipe(
      map((result) => {
        if (!result.succeeded || !result.data) {
          throw new Error(result.error?.description ?? 'Failed to load polls');
        }
        return result.data;
      })
    );
  }

  getCurrentPolls(): Observable<PollResponse[]> {
    return this.http.get<PollResponse[]>(`${this.apiUrl}/current`);
  }

  getPoll(id: string): Observable<Poll> {
    return this.http.get<Poll>(`${this.apiUrl}/${id}`);
  }

  createPoll(request: CreatePollRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }

  updatePoll(id: string, request: UpdatePollRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  deletePoll(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  togglePublish(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/togglePublish`, {});
  }

  getStats(): Observable<PollStatsResponse> {
    return this.http.get<PollStatsResponse>(`${this.apiUrl}/stats`);
  }
}
