import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const ownApi = req.url.startsWith('/') && !req.url.startsWith('/dev/token');
  if (!ownApi) return next(req);

  return from(auth.getAccessToken()).pipe(
    switchMap(token => {
      const outgoing = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
      return next(outgoing);
    }),
    catchError((e: unknown) => {
      if (e instanceof HttpErrorResponse && e.status === 401) {
        auth.logout();
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      }
      return throwError(() => e);
    }),
  );
};
