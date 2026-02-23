import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { Question, QuestionOption, QuestionRequest } from '../models/question';

@Injectable({
  providedIn: 'root'
})
export class QuestionService {
  private apiUrl = `${API_BASE_URL}/polls`;

  constructor(private http: HttpClient) {}

  getQuestions(pollId: string): Observable<Question[]> {
    return this.http.get<Question[]>(`${this.apiUrl}/${pollId}/questions`).pipe(
      map((questions) =>
        questions.map((q) => ({
          ...q,
          answers: this.normalizeAnswers(q.answers)
        }))
      )
    );
  }

  private normalizeAnswers(rawAnswers: unknown): QuestionOption[] {
    if (!Array.isArray(rawAnswers)) {
      return [];
    }

    return rawAnswers
      .map((answer, index) => {
        if (typeof answer === 'string') {
          return {
            id: `option-${index + 1}`,
            content: answer
          };
        }

        if (answer && typeof answer === 'object') {
          const candidate = answer as { id?: unknown; content?: unknown };
          if (typeof candidate.content === 'string') {
            return {
              id: typeof candidate.id === 'string' ? candidate.id : `option-${index + 1}`,
              content: candidate.content
            };
          }
        }

        return null;
      })
      .filter((answer): answer is QuestionOption => !!answer);
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
