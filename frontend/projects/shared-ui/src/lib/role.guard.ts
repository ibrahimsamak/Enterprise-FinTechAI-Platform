import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService, Role } from './auth.service';

export const roleGuard = (role: Role): CanActivateFn => (_route, state) =>
  inject(AuthService).hasRole(role)
    ? true
    : inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
