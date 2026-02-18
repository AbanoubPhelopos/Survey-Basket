import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { QuestionResponse, VoteRequest } from '../models/vote';

@Injectable({
  providedIn: 'root'
})
export class VoteService {
  private apiUrl = `${API_BASE_URL}/polls`;

  constructor(private http: HttpClient) {}

  startVote(pollId: string): Observable<QuestionResponse[]> {
    return this.http.get<QuestionResponse[]>(`${this.apiUrl}/${pollId}/votes`);
  }

  submitVote(pollId: string, request: VoteRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${pollId}/votes`, request);
  }
}
