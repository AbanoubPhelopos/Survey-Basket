import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { RoleResponse } from '../models/role';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly apiUrl = `${API_BASE_URL}/roles`;

  constructor(private readonly http: HttpClient) {}

  getRoles(includeDisabled = false): Observable<RoleResponse[]> {
    return this.http.get<RoleResponse[]>(`${this.apiUrl}?includeDisabled=${includeDisabled}`);
  }
}
