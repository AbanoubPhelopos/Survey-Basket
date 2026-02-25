import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import {
  ActivateCompanyAccountRequest,
  CompanyMagicLinkRedeemRequest,
  CompanyPollAccessRedeemRequest,
  CompanyUserInviteRedeemRequest,
  LoginRequest,
  LoginResponse,
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

  activateCompanyAccount(companyId: string, request: ActivateCompanyAccountRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/activate-company`, {
      companyAccountUserId: companyId,
      activationToken: request.activationToken,
      newPassword: request.newPassword
    });
  }

  requestCompanyMagicLink(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/company/magic-link/request`, { email });
  }

  redeemCompanyMagicLink(request: CompanyMagicLinkRedeemRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/company/magic-link/redeem`, request).pipe(
      tap((response) => this.setSession(response))
    );
  }

  redeemCompanyUserInvite(request: CompanyUserInviteRedeemRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/company-user/invite/redeem`, request).pipe(
      tap((response) => this.setSession(response))
    );
  }

  redeemCompanyPollAccess(request: CompanyPollAccessRedeemRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/company-user/poll-access/redeem`, request).pipe(
      tap((response) => this.setSession(response))
    );
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

  requiresProfileCompletion(): boolean {
    return !!this.user()?.requiresProfileCompletion;
  }

  requiresPasswordSetup(): boolean {
    return !!this.user()?.requiresPasswordSetup;
  }

  markProfileCompleted(): void {
    const current = this.user();
    if (!current) return;
    const next = { ...current, requiresProfileCompletion: false };
    localStorage.setItem(this.userKey, JSON.stringify(next));
    this.user.set(next);
  }

  markPasswordSetupCompleted(): void {
    const current = this.user();
    if (!current) return;
    const next = { ...current, requiresPasswordSetup: false, redirectPollId: undefined };
    localStorage.setItem(this.userKey, JSON.stringify(next));
    this.user.set(next);
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
      requiresActivation: authResult.requiresActivation,
      requiresProfileCompletion: authResult.requiresProfileCompletion,
      requiresPasswordSetup: authResult.requiresPasswordSetup,
      redirectPollId: authResult.redirectPollId
    };
    
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.user.set(user);
  }
}
