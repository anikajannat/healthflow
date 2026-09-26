import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { Role } from './models';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService); const router = inject(Router);
  return auth.isLoggedIn() ? true : router.createUrlTree(['/login']);
};

export const roleGuard = (roles: Role[]): CanActivateFn => () => {
  const auth = inject(AuthService); const router = inject(Router);
  return auth.user() && roles.includes(auth.user()!.role) ? true : router.createUrlTree(['/']);
};
