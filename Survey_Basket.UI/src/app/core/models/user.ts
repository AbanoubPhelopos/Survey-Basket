export interface UserResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  isDisabled: boolean;
  roles: string[];
}

export interface UsersStatsResponse {
  totalUsers: number;
  activeUsers: number;
  disabledUsers: number;
  distinctRoles: number;
}
