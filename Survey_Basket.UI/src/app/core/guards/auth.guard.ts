import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const requiredRoles = (route.data?.['roles'] as string[] | undefined) ?? [];
  const requiredPermissions = (route.data?.['permissions'] as string[] | undefined) ?? [];

  if (authService.isAuthenticated()) {
    if (authService.requiresActivation() && !state.url.startsWith('/activate-company')) {
      return router.createUrlTree(['/activate-company']);
    }

    if (requiredRoles.length > 0 && !authService.hasAnyRole(requiredRoles)) {
      return router.createUrlTree(['/dashboard']);
    }

    if (requiredPermissions.length > 0 && !authService.hasAnyPermission(requiredPermissions)) {
      return router.createUrlTree(['/dashboard']);
    }

    return true;
  }

  return router.createUrlTree(['/login']);
};
