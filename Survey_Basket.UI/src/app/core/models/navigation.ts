import { AppRole } from './auth';

export interface NavItem {
  label: string;
  route: string;
  roles?: AppRole[];
  permissions?: string[];
}

export const APP_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', route: '/dashboard' },
  { label: 'Polls', route: '/polls/new', roles: ['Admin', 'SystemAdmin', 'PartnerCompany'] },
  { label: 'Users', route: '/users', roles: ['Admin', 'SystemAdmin'] },
  { label: 'Companies', route: '/admin/companies', roles: ['Admin', 'SystemAdmin'] },
  { label: 'Company Users', route: '/company/users', roles: ['PartnerCompany'] },
  { label: 'Roles', route: '/admin/roles', roles: ['Admin', 'SystemAdmin'] },
  { label: 'Profile', route: '/profile' }
];
