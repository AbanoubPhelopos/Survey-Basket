export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  refreshTokenExpiration: Date;
  expiresIn: number;
  roles: string[];
  permissions: string[];
  firstName: string;
  lastName: string;
  email: string;
  userId: string;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  permissions: string[];
  roles: string[];
}
