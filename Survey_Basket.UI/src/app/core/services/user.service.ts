import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { UserResponse } from '../models/user';
import { CreateCompanyUserRecordRequest, CreateCompanyUserRecordResponse } from '../models/company-user-record';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${API_BASE_URL}/users`;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(this.apiUrl);
  }

  createCompanyUserRecord(request: CreateCompanyUserRecordRequest): Observable<CreateCompanyUserRecordResponse> {
    return this.http.post<CreateCompanyUserRecordResponse>(`${this.apiUrl}/company-user-records`, request);
  }
}
