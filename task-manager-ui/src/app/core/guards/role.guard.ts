import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const requiredRole = route.data['role'];

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  const userRole = authService.getRole();
  if (!userRole || userRole !== requiredRole) {
    console.log(userRole, requiredRole);
    return router.createUrlTree(['/unauthorized']);
  }

  return true;
};
