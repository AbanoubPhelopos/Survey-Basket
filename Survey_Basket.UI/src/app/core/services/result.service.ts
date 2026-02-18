import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { PollVotesResponse, VotesPerDayResponse, VotesPerQuestionResponse } from '../models/result';

@Injectable({
  providedIn: 'root'
})
export class ResultService {
  private apiUrl = `${API_BASE_URL}/polls`;

  constructor(private http: HttpClient) {}

  getPollVotes(pollId: string): Observable<PollVotesResponse> {
    return this.http.get<PollVotesResponse>(`${this.apiUrl}/${pollId}/results/row-data`);
  }

  getVotesPerDay(pollId: string): Observable<VotesPerDayResponse[]> {
    return this.http.get<VotesPerDayResponse[]>(`${this.apiUrl}/${pollId}/results/votes-per-day`);
  }

  getVotesPerQuestion(pollId: string): Observable<VotesPerQuestionResponse[]> {
    return this.http.get<VotesPerQuestionResponse[]>(`${this.apiUrl}/${pollId}/results/votes-per-question`);
  }
}
