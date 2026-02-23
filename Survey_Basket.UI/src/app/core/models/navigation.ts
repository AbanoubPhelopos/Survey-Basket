import { AppRole } from './auth';

export interface NavItem {
  label: string;
  route: string;
  icon: 'dashboard' | 'polls' | 'users' | 'companies' | 'companyUsers' | 'roles' | 'profile';
  roles?: AppRole[];
  permissions?: string[];
}

export const APP_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
  { label: 'Polls', route: '/polls/new', icon: 'polls', roles: ['Admin', 'SystemAdmin', 'PartnerCompany'] },
  { label: 'Users', route: '/users', icon: 'users', roles: ['Admin', 'SystemAdmin'] },
  { label: 'Companies', route: '/admin/companies', icon: 'companies', roles: ['Admin', 'SystemAdmin'] },
  { label: 'Company Users', route: '/company/users', icon: 'companyUsers', roles: ['PartnerCompany'] },
  { label: 'Roles', route: '/admin/roles', icon: 'roles', roles: ['Admin', 'SystemAdmin'] },
  { label: 'Profile', route: '/profile', icon: 'profile' }
];
