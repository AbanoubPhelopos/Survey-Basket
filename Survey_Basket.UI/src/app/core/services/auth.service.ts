import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import {
  ActivateCompanyAccountRequest,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  User
} from '../models/auth';
import { API_BASE_URL } from '../constants/api.constants';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${API_BASE_URL}/auth`;
  private tokenKey = 'sb_token';
  private userKey = 'sb_user';

  user = signal<User | null>(this.getUser());

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}`, request).pipe(
      tap(response => {
        this.setSession(response);
      })
    );
  }

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, request);
  }

  activateCompanyAccount(companyId: string, request: ActivateCompanyAccountRequest): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/companies/${companyId}/activate`, request);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.user.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUser(): User | null {
    const user = localStorage.getItem(this.userKey);
    return user ? JSON.parse(user) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    const roles = this.user()?.roles ?? [];
    return roles.some(r => r.toLowerCase() === role.toLowerCase());
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  hasPermission(permission: string): boolean {
    const permissions = this.user()?.permissions ?? [];
    return permissions.includes(permission);
  }

  hasAnyPermission(permissions: string[]): boolean {
    return permissions.some(permission => this.hasPermission(permission));
  }

  isAdminContext(): boolean {
    return this.hasAnyRole(['Admin', 'SystemAdmin']) || this.user()?.accountType === 'AdminAccount';
  }

  isCompanyAccountContext(): boolean {
    return this.hasRole('PartnerCompany') || this.user()?.accountType === 'CompanyAccount';
  }

  requiresActivation(): boolean {
    return !!this.user()?.requiresActivation;
  }

  private setSession(authResult: LoginResponse): void {
    localStorage.setItem(this.tokenKey, authResult.token);
    
    const user: User = { 
      id: authResult.userId, 
      email: authResult.email, 
      firstName: authResult.firstName, 
      lastName: authResult.lastName, 
      permissions: authResult.permissions || [],
      roles: authResult.roles || [],
      accountType: authResult.accountType,
      requiresActivation: authResult.requiresActivation
    };
    
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.user.set(user);
  }
}
