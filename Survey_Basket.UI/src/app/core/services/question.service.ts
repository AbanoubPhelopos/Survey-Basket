import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { Question, QuestionRequest } from '../models/question';

@Injectable({
  providedIn: 'root'
})
export class QuestionService {
  private apiUrl = `${API_BASE_URL}/polls`;

  constructor(private http: HttpClient) {}

  getQuestions(pollId: string): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.apiUrl}/${pollId}/questions`);
  }

  addQuestion(pollId: string, request: QuestionRequest): Observable<Question> {
    return this.http.post<Question>(`${this.apiUrl}/${pollId}/questions`, request);
  }

  updateQuestion(pollId: string, questionId: string, request: QuestionRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${pollId}/questions/${questionId}`, request);
  }

  toggleStatus(pollId: string, questionId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${pollId}/questions/${questionId}/toggle-status`, {});
  }
}
