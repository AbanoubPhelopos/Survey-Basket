import { AppRole } from './auth';

export interface NavItem {
  label: string;
  route: string;
  roles?: AppRole[];
  permissions?: string[];
}

export const APP_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', route: '/dashboard' },
  { label: 'Polls', route: '/polls', roles: ['Admin', 'SystemAdmin', 'PartnerCompany', 'Member'] },
  { label: 'Users', route: '/users', roles: ['Admin', 'SystemAdmin'] },
  { label: 'Profile', route: '/profile' }
];
