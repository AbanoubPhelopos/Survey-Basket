import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { QuestionResponse, VoteRequest } from '../models/vote';
import { ServiceResult } from '../models/service-result';

@Injectable({
  providedIn: 'root'
})
export class VoteService {
  private apiUrl = `${API_BASE_URL}/polls`;

  constructor(private http: HttpClient) {}

  startVote(pollId: string): Observable<QuestionResponse[]> {
    return this.http.get<ServiceResult<QuestionResponse[]>>(`${this.apiUrl}/${pollId}/votes`).pipe(
      map((result) => {
        if (!result.succeeded || !result.data) {
          throw new Error(result.error?.description ?? 'Failed to load vote questions');
        }
        return result.data;
      })
    );
  }

  submitVote(pollId: string, request: VoteRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${pollId}/votes`, request);
  }
}
