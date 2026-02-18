import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { UserProfileResponse, UpdateProfileRequest, ChangePasswordRequest } from '../models/account';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private apiUrl = `${API_BASE_URL.replace('/api', '')}/me`; // The controller route is 'me', not 'api/me' based on code?
  // Wait, let's check Route attribute.
  // [Route("me")] -> likely relative to root if not starting with /.
  // But Program.cs maps controllers. Usually it's relative to base path?
  // No, if Route is "me", it's "/me".
  // But usually APIs are grouped under /api.
  // Let's assume it is `/me`.
  
  // Actually, standard is `/me`.
  // But wait, the backend uses `app.MapControllers()`.
  // If `[Route("me")]` is used, it's at root `/me`.
  // If `[Route("api/[controller]")]` is used, it's `/api/...`
  
  // I will check if there is a global prefix.
  // Program.cs doesn't show global prefix config.
  // So it is likely `http://localhost:5002/me`.
  
  // BUT, usually people put `[Route("api/me")]`.
  // The code says `[Route("me")]`.
  
  // Let's assume `http://localhost:5002/me`.
  
  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfileResponse> {
    return this.http.get<UserProfileResponse>('http://localhost:5002/me');
  }

  updateProfile(request: UpdateProfileRequest): Observable<void> {
    return this.http.put<void>('http://localhost:5002/me/info', request);
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.put<void>('http://localhost:5002/me/change-password', request);
  }
}
