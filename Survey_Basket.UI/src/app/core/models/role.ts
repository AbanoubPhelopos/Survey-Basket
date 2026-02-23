export interface RoleResponse {
  id: string;
  name: string;
  isDeleted: boolean;
}

export interface RoleStatsResponse {
  totalRoles: number;
  activeRoles: number;
  disabledRoles: number;
  permissionLinks: number;
}
